#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Net;
using Convex.IRC.Net.Event;
using Convex.Plugin;
using Convex.Plugin.Registrar;
using Convex.Util;
using Newtonsoft.Json;

#endregion

namespace Convex.IRC {
    public sealed class IrcClient : IDisposable, IIrcClient {
        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     receiving.
        /// </summary>
        public IrcClient(Func<ServerMessage, string> formatter, Func<ServerMessagedEventArgs, Task> invokeAsyncMethod, IConfiguration config = null) {
            Initialising = true;

            _pendingPlugins = new Stack<IAsyncRegistrar<ServerMessagedEventArgs>>();

            UniqueId = Guid.NewGuid();
            Server = new Server(formatter);

            TerminateSignaled += Terminate;
            Server.ServerMessaged += OnServerMessaged;

            InitialiseConfiguration(config);

            ServerMessagedHostWrapper = new PluginHostWrapper<ServerMessagedEventArgs>(Configuration.DefaultPluginDirectoryPath, invokeAsyncMethod, "Convex.*.dll");
            ServerMessagedHostWrapper.TerminateSignaled += OnTerminateSignaled;
            ServerMessagedHostWrapper.CommandReceived += Server.Connection.SendDataAsync;

            Initialising = false;
        }

        #region INTERFACE IMPLEMENTATION

        /// <inheritdoc />
        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose() {
            Dispose(true).Wait();
        }

        private async Task Dispose(bool dispose) {
            if (!dispose || _disposed) {
                return;
            }

            await ServerMessagedHostWrapper.Host.StopPlugins();
            Server?.Dispose();
            Config?.Dispose();

            _disposed = true;
        }

        #endregion

        #region MEMBERS

        public Guid UniqueId { get; }
        public IServer Server { get; }
        public IConfiguration Config { get; private set; }
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

        public List<string> IgnoreList => Config.IgnoreList ?? new List<string>();
        public Dictionary<string, Tuple<string, string>> LoadedCommands => ServerMessagedHostWrapper.Host.DescriptionRegistry;

        public string Address => Server.Connection.Address.Hostname;
        public int Port => Server.Connection.Address.Port;
        public bool IsInitialised { get; private set; }
        public bool Initialising { get; private set; }

        private PluginHostWrapper<ServerMessagedEventArgs> ServerMessagedHostWrapper { get; }

        private bool _disposed;

        private Stack<IAsyncRegistrar<ServerMessagedEventArgs>> _pendingPlugins;

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync() {
            do {
                await Server.ListenAsync(this);
            } while (Server.Connection.IsConnected);
        }

        private async Task OnServerMessaged(object source, ServerMessagedEventArgs args) {
            if (string.IsNullOrEmpty(args.Message.Command)) {
                return;
            }

            if (args.Message.Command.Equals(Commands.ERROR)) {
                return;
            }

            if (args.Message.Nickname.Equals(Config.Nickname) || Config.IgnoreList.Contains(args.Message.Realname)) {
                return;
            }

            if (args.Message.SplitArgs.Count >= 2 && args.Message.SplitArgs[0].Equals(Config.Nickname.ToLower())) {
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();
            }

            try {
                await ServerMessagedHostWrapper.Host.InvokeAsync(this, args);
            } catch (Exception ex) {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }

        #endregion

        #region INIT

        private void InitialiseConfiguration(IConfiguration configuration) {
            if (!Directory.Exists(Configuration.DefaultResourceDirectory)) {
                Directory.CreateDirectory(Configuration.DefaultResourceDirectory);
            }

            if (configuration == null) {
                Configuration.CheckCreateConfig(Configuration.DefaultConfigurationFilePath);
                Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Configuration.DefaultConfigurationFilePath));
            } else {
                Config = configuration;
            }
        }

        public async Task<bool> Initialise(IAddress address) {
            if (IsInitialised || Initialising) {
                return true;
            }

            Initialising = true;

            await InitialisePluginWrapper();

            RegisterMethods();

            await Server.Initialise(address);

            await OnInitialised(this, new ClassInitialisedEventArgs(this));

            await Server.SendConnectionInfo(Config.Nickname, Config.Realname);

            Initialising = false;

            return IsInitialised = Server.Initialised && ServerMessagedHostWrapper.Initialised;
        }

        private async Task InitialisePluginWrapper() {
            await ServerMessagedHostWrapper.Initialise();
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<ErrorEventArgs> Error;

        private async Task OnQuery(object source, DatabaseQueriedEventArgs args) {
            if (Queried == null) {
                return;
            }

            await Queried.Invoke(source, args);
        }

        private async Task OnTerminateSignaled(object source, OperationTerminatedEventArgs args) {
            if (TerminateSignaled == null) {
                return;
            }

            await TerminateSignaled.Invoke(source, args);
        }

        private async Task OnInitialised(object source, ClassInitialisedEventArgs args) {
            if (Initialised == null) {
                return;
            }

            await Initialised.Invoke(source, args);
        }

        private async Task OnError(object source, ErrorEventArgs args) {
            if (Error == null) {
                return;
            }

            await Error.Invoke(source, args);
        }

        private async Task Terminate(object source, OperationTerminatedEventArgs args) {
            await Dispose(true);
        }

        #endregion

        #region METHODS

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            if (!IsInitialised && !Initialising) {
                _pendingPlugins.Push(methodRegistrar);

                return;
            }

            ServerMessagedHostWrapper.Host.RegisterMethod(methodRegistrar);
        }

        private void RegisterMethods() {
            while (_pendingPlugins.Count > 0) {
                RegisterMethod(_pendingPlugins.Pop());
            }
        }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public Tuple<string, string> GetCommand(string command) {
            return ServerMessagedHostWrapper.Host.DescriptionRegistry.Values.SingleOrDefault(x => x != null && x.Item1.Equals(command, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) {
            return !GetCommand(command).Equals(default(Tuple<string, string>));
        }

        public string GetApiKey(string type) {
            return Config.ApiKeys[type];
        }

        #endregion
    }
}