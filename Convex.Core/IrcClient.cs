#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Core.Configuration;
using Convex.Core.Net;
using Convex.Event;
using Convex.Plugin;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Convex.Util;

#endregion

namespace Convex.Core
{
    public sealed class IrcClient : IDisposable, IIrcClient
    {
        /// <summary>
        ///     Initializes class. No connections are made at initialization, so call `Initialize()` to begin sending and
        ///     receiving.
        /// </summary>
        public IrcClient(Func<ServerMessage, string> formatter,
            Func<InvokedAsyncEventArgs<ServerMessagedEventArgs>, Task> invokeAsyncMethod)
        {
            Initializing = true;

            Version = new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

            _PendingPlugins = new Stack<IAsyncCompsition<ServerMessagedEventArgs>>();

            UniqueId = Guid.NewGuid();
            Server = new Server(formatter);

            TerminateSignaled += Terminate;
            Server.ServerMessaged += OnServerMessaged;

            ServerMessagedHostWrapper =
                new PluginHostWrapper<ServerMessagedEventArgs>(Config.GetProperty("PluginsDirectory").ToString(),
                    invokeAsyncMethod, "Convex.*.dll");
            ServerMessagedHostWrapper.TerminateSignaled += OnTerminateSignaled;
            ServerMessagedHostWrapper.CommandReceived += Server.Connection.SendDataAsync;

            Initializing = false;
        }

        #region INTERFACE IMPLEMENTATION

        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose()
        {
            Dispose(true).Wait();
        }

        private async Task Dispose(bool dispose)
        {
            if (!dispose || _Disposed)
            {
                return;
            }

            await ServerMessagedHostWrapper.Host.StopPlugins();
            Server?.Dispose();

            _Disposed = true;
        }

        #endregion

        #region MEMBERS

        public Guid UniqueId { get; }
        public Server Server { get; }
        public Version Version { get; }

        public Dictionary<string, CompositionDescription> LoadedDescriptions =>
            ServerMessagedHostWrapper.Host.DescriptionRegistry;

        public string Address => Server.Connection.Address.Hostname;
        public int Port => Server.Connection.Address.Port;
        public bool IsInitialized { get; private set; }
        public bool Initializing { get; private set; }

        private PluginHostWrapper<ServerMessagedEventArgs> ServerMessagedHostWrapper { get; }

        private bool _Disposed;

        private readonly Stack<IAsyncCompsition<ServerMessagedEventArgs>> _PendingPlugins;

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync()
        {
            do
            {
                await Server.ListenAsync(this);
            } while (Server.Connection.IsConnected);
        }

        private async Task OnServerMessaged(object source, ServerMessagedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Message.Command))
            {
                return;
            }

            if (args.Message.Command.Equals(Commands.ERROR))
            {
                return;
            }

            if (args.Message.Nickname.Equals(Config.GetProperty("Nickname").ToString())
                || ((List<string>)Config.GetProperty("IgnoreList")).Contains(args.Message.RealName))
            {
                return;
            }

            if ((args.Message.SplitArgs.Count >= 2)
                && args.Message.SplitArgs[0].Equals(Config.GetProperty("Nickname").ToString().ToLower()))
            {
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();
            }

            try
            {
                await ServerMessagedHostWrapper.Host.InvokeAsync(this, args);
            }
            catch (Exception ex)
            {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }

        #endregion

        #region INIT

        public async Task<bool> Initialize(IAddress address)
        {
            if (IsInitialized || Initializing)
            {
                return true;
            }

            Initializing = true;

            await InitializePluginWrapper();

            RegisterMethods();

            await Server.Initialize(address);

            await OnInitialized(this, new ClassInitializedEventArgs(this));

            await Server.SendIdentityInfo(Config.GetProperty("Nickname").ToString(),
                Config.GetProperty("Realname").ToString());

            Initializing = false;

            return IsInitialized = Server.Initialized && ServerMessagedHostWrapper.Initialized;
        }

        private async Task InitializePluginWrapper()
        {
            await ServerMessagedHostWrapper.Initialize();
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        public event AsyncEventHandler<ClassInitializedEventArgs> Initialized;
        public event AsyncEventHandler<ErrorEventArgs> Error;

        private async Task OnQuery(object source, DatabaseQueriedEventArgs args)
        {
            if (Queried == null)
            {
                return;
            }

            await Queried.Invoke(source, args);
        }

        private async Task OnTerminateSignaled(object source, OperationTerminatedEventArgs args)
        {
            if (TerminateSignaled == null)
            {
                return;
            }

            await TerminateSignaled.Invoke(source, args);
        }

        private async Task OnInitialized(object source, ClassInitializedEventArgs args)
        {
            if (Initialized == null)
            {
                return;
            }

            await Initialized.Invoke(source, args);
        }

        private async Task OnError(object source, ErrorEventArgs args)
        {
            if (Error == null)
            {
                return;
            }

            await Error.Invoke(source, args);
        }

        private async Task Terminate(object source, OperationTerminatedEventArgs args)
        {
            await Dispose(true);
        }

        #endregion

        #region METHODS

        public void RegisterMethod(IAsyncCompsition<ServerMessagedEventArgs> methodRegistrar)
        {
            if (!IsInitialized && !Initializing)
            {
                _PendingPlugins.Push(methodRegistrar);

                return;
            }

            ServerMessagedHostWrapper.Host.RegisterComposition(methodRegistrar);
        }

        private void RegisterMethods()
        {
            while (_PendingPlugins.Count > 0)
            {
                RegisterMethod(_PendingPlugins.Pop());
            }
        }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public CompositionDescription GetDescription(string command)
        {
            return ServerMessagedHostWrapper.Host.DescriptionRegistry.Values.SingleOrDefault(kvp =>
                kvp.Command.Equals(command, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) => !GetDescription(command).Equals(default(Tuple<string, string>));

        #endregion
    }
}
