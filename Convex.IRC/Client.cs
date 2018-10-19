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
using Convex.IRC.Dependency;
using Convex.Plugin;
using Convex.Plugin.Registrar;
using Newtonsoft.Json;

#endregion

namespace Convex.IRC {
    public sealed class Client : IDisposable, IClient {
        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     receiving.
        /// </summary>
        public Client(Configuration configuration = null) {
            Initialising = true;

            UniqueId = Guid.NewGuid();
            Server = new Server();

            TerminateSignaled += Terminate;
            Server.ServerMessaged += OnServerMessaged;

            InitialiseConfiguration(configuration);

            Wrapper = new PluginWrapper<ServerMessagedEventArgs>($@"{AppContext.BaseDirectory}\Plugins", OnInvokedMethod);
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

        #endregion

        private async Task Dispose(bool dispose) {
            if (!dispose || _disposed)
                return;

            await Wrapper.Host.StopPlugins();
            Server?.Dispose();
            GetClientConfiguration()?.Dispose();

            _disposed = true;
        }

        #region MEMBERS

        public Guid UniqueId { get; }

        private PluginWrapper<ServerMessagedEventArgs> Wrapper { get; }

        public bool IsInitialised { get; private set; }
        public bool Initialising { get; private set; }

        public Server Server { get; }

        private Configuration _clientConfiguration;

        public Configuration GetClientConfiguration() {
            return _clientConfiguration;
        }

        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

        public Dictionary<string, Tuple<string, string>> LoadedCommands => Wrapper.Host.DescriptionRegistry;
        public string Address => Server.Connection.Address;
        public int Port => Server.Connection.Port;
        public List<string> IgnoreList => GetClientConfiguration().IgnoreList ?? new List<string>();

        private bool _disposed;

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync() {
            do {
                await Server.ListenAsync(this);
            } while (Server.Connection.IsConnected);
        }

        private async Task OnServerMessaged(object source, ServerMessagedEventArgs args) {
            if (string.IsNullOrEmpty(args.Message.Command))
                return;

            if (args.Message.Command.Equals(Commands.PRIVMSG)) {
                if (args.Message.Origin.StartsWith("#"))
                    Server.Channels.Add(new Channel(args.Message.Origin));
            } else if (args.Message.Command.Equals(Commands.ERROR)) {
                return;
            }

            if (args.Message.Nickname.Equals(GetClientConfiguration().Nickname) || GetClientConfiguration().IgnoreList.Contains(args.Message.Realname))
                return;

            if (args.Message.SplitArgs.Count >= 2 && args.Message.SplitArgs[0].Equals(GetClientConfiguration().Nickname.ToLower()))
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();

            try {
                await Wrapper.Host.InvokeAsync(args);
            } catch (Exception ex) {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }

        #endregion

        #region INIT

        private void InitialiseConfiguration(Configuration configuration) {
            if (!Directory.Exists(Configuration.DefaultResourceDirectory))
                Directory.CreateDirectory(Configuration.DefaultResourceDirectory);

            if (configuration == null) {
                Configuration.CheckCreateConfig(Configuration.DefaultConfigurationFilePath);
                _clientConfiguration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Configuration.DefaultConfigurationFilePath));
            } else {
                _clientConfiguration = configuration;
            }
        }

        public async Task<bool> Initialise(string address, int port) {
            if (IsInitialised || Initialising) return true;

            Initialising = true;

            await InitialisePluginWrapper();

            await Server.Initialise(address, port);

            await OnInitialised(this, new ClassInitialisedEventArgs(this));

            await Server.SendConnectionInfo(GetClientConfiguration().Nickname, GetClientConfiguration().Realname);

            Initialising = false;

            return IsInitialised = Server.Initialised && Wrapper.Initialised;
        }

        private async Task InitialisePluginWrapper() {
            await Wrapper.Initialise();

            RegisterMethods();
        }

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods() {
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Nick, null, Commands.NICK, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Join, null, Commands.JOIN, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Part, null, Commands.PART, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(ChannelTopic, null, Commands.CHANNEL_TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NewTopic, null, Commands.TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NamesReply, null, Commands.NAMES_REPLY, null));
        }

        public void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar) {
            Wrapper.Host.RegisterMethod(methodRegistrar);
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<InformationLoggedEventArgs> Logged;
        public event AsyncEventHandler<ErrorEventArgs> Error;

        private async Task OnQuery(object sender, DatabaseQueriedEventArgs args) {
            if (Queried == null)
                return;

            await Queried.Invoke(sender, args);
        }

        private async Task OnTerminateSignaled(object sender, OperationTerminatedEventArgs args) {
            if (TerminateSignaled == null)
                return;

            await TerminateSignaled.Invoke(sender, args);
        }

        private async Task OnInitialised(object sender, ClassInitialisedEventArgs args) {
            if (Initialised == null)
                return;

            await Initialised.Invoke(sender, args);
        }

        private async Task OnError(object sender, ErrorEventArgs args) {
            if (Error == null)
                return;

            await Error.Invoke(sender, args);
        }

        private async Task OnLog(object sender, InformationLoggedEventArgs args) {
            if (Logged == null)
                return;

            await Logged.Invoke(sender, args);
        }

        private async Task Terminate(object sender, OperationTerminatedEventArgs args) {
            await Dispose(true);
        }

        #endregion

        #region METHODS

        //public IEnumerable<User> GetAllUsers() {
        //    return MainDatabase.Users;
        //}

        ///// <summary>
        /////     Gets the user entry by their realname
        /////     note: will return null if user does not exist
        ///// </summary>
        ///// <param name="realname">realname of user</param>
        //public User GetUser(string realname) {
        //    return MainDatabase.Users.SingleOrDefault(user => user.Realname.Equals(realname));
        //}

        ///// <summary>
        /////     Checks whether a user exists in database already
        ///// </summary>
        ///// <param name="userName">name of user</param>
        ///// <returns></returns>
        //public bool UserExists(string userName) {
        //    return MainDatabase.Users.Any(user => user.Realname.Equals(userName));
        //}

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
            return GetClientConfiguration().ApiKeys[type];
        }

        private async Task OnInvokedMethod(ServerMessagedEventArgs args) {
            if (!Wrapper.Host.CompositionHandlers.ContainsKey(args.Message.Command))
                return;

            await Wrapper.Host.CompositionHandlers[args.Message.Command].Invoke(this, args);
        }

        #endregion

        #region REGISTRARS

        private async Task Nick(ServerMessagedEventArgs e) {
            await OnQuery(this, new DatabaseQueriedEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));
        }

        private Task Join(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.Add(e.Message.Nickname);

            return Task.CompletedTask;
        }

        private Task Part(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin)?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));

            return Task.CompletedTask;
        }

        private Task ChannelTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);

            return Task.CompletedTask;
        }

        private Task NewTopic(ServerMessagedEventArgs e) {
            Server.GetChannel(e.Message.Origin).Topic = e.Message.Args;

            return Task.CompletedTask;
        }

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4)
                return Task.CompletedTask;

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Server.Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null || currentChannel.Inhabitants.Contains(s))
                    continue;

                Server?.Channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}