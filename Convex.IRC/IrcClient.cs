#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.IRC.Component.Reference;
using Convex.Plugin;
using Convex.Plugin.Registrar;
using Newtonsoft.Json;

#endregion

namespace Convex.IRC {
    public sealed class IrcClient : IDisposable, IIrcClient {
        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     receiving.
        /// </summary>
        public IrcClient(Configuration config = null) {
            Initialising = true;

            UniqueId = Guid.NewGuid();
            Server = new Server();

            TerminateSignaled += Terminate;
            Server.ServerMessaged += OnServerMessaged;

            InitialiseConfiguration(config);

            Wrapper = new PluginWrapper<ServerMessagedEventArgs>(Configuration.DefaultPluginDirectoryPath, OnInvokedMethod);
            Wrapper.Logged += OnLog;
            Wrapper.TerminateSignaled += OnTerminateSignaled;
            Wrapper.CommandReceived += Server.Connection.SendDataAsync;

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

            await Wrapper.Host.StopPlugins();
            Server?.Dispose();
            Config?.Dispose();

            _disposed = true;
        }

        #endregion

        #region MEMBERS

        public Guid UniqueId { get; }
        public Server Server { get; }
        public Configuration Config { get; private set; }
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

        public List<string> IgnoreList => Config.IgnoreList ?? new List<string>();
        public Dictionary<string, Tuple<string, string>> LoadedCommands => Wrapper.Host.DescriptionRegistry;

        public string Address => Server.Connection.Address;
        public int Port => Server.Connection.Port;
        public bool IsInitialised { get; private set; }
        public bool Initialising { get; private set; }

        private PluginWrapper<ServerMessagedEventArgs> Wrapper { get; }

        private bool _disposed;

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
                await Wrapper.Host.InvokeAsync(args);
            } catch (Exception ex) {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }

        #endregion

        #region INIT

        private void InitialiseConfiguration(Configuration configuration) {
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

        public async Task<bool> Initialise(string address, int port) {
            if (IsInitialised || Initialising) {
                return true;
            }

            Initialising = true;

            await InitialisePluginWrapper();

            await Server.Initialise(address, port);

            await OnInitialised(this, new ClassInitialisedEventArgs(this));

            await Server.SendConnectionInfo(Config.Nickname, Config.Realname);

            Initialising = false;

            return IsInitialised = Server.Initialised && Wrapper.Initialised;
        }

        private async Task InitialisePluginWrapper() {
            await Wrapper.Initialise();
        }

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            Wrapper.Host.RegisterMethod(methodRegistrar);
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<LogEventArgs> Logged;
        public event AsyncEventHandler<ErrorEventArgs> Error;

        private async Task OnQuery(object sender, DatabaseQueriedEventArgs args) {
            if (Queried == null) {
                return;
            }

            await Queried.Invoke(sender, args);
        }

        private async Task OnTerminateSignaled(object sender, OperationTerminatedEventArgs args) {
            if (TerminateSignaled == null) {
                return;
            }

            await TerminateSignaled.Invoke(sender, args);
        }

        private async Task OnInitialised(object sender, ClassInitialisedEventArgs args) {
            if (Initialised == null) {
                return;
            }

            await Initialised.Invoke(sender, args);
        }

        private async Task OnError(object sender, ErrorEventArgs args) {
            if (Error == null) {
                return;
            }

            await Error.Invoke(sender, args);
        }

        private async Task OnLog(object sender, LogEventArgs args) {
            if (Logged == null) {
                return;
            }

            await Logged.Invoke(sender, args);
        }

        private async Task Terminate(object sender, OperationTerminatedEventArgs args) {
            await Dispose(true);
        }

        #endregion

        #region METHODS

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public Tuple<string, string> GetCommand(string command) {
            return Wrapper.Host.DescriptionRegistry.Values.SingleOrDefault(x => x != null && x.Item1.Equals(command, StringComparison.CurrentCultureIgnoreCase));
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

        private async Task OnInvokedMethod(ServerMessagedEventArgs args) {
            if (!Wrapper.Host.CompositionHandlers.ContainsKey(args.Message.Command)) {
                return;
            }

            await Wrapper.Host.CompositionHandlers[args.Message.Command].Invoke(this, args);
        }

        #endregion
    }
}