#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.ComponentModel.Event;
using Convex.IRC.ComponentModel.Reference;
using Convex.IRC.Models;
using Convex.IRC.Models.Net;
using Convex.Plugin;
using Convex.Plugin.Registrar;
using Newtonsoft.Json;

#endregion

namespace Convex.IRC {
    public sealed class Client : IDisposable {
        #region MEMBERS

        private PluginWrapper<ServerMessagedEventArgs> Wrapper { get; }

        public bool IsInitialised { get; private set; }

        public Server Server { get; }
        public Configuration ClientConfiguration { get; }
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

        public Dictionary<string, Tuple<string, string>> LoadedCommands => Wrapper.Host.DescriptionRegistry;
        public List<string> IgnoreList => ClientConfiguration.IgnoreList;

        private bool _disposed;

        #endregion

        /// <summary>
        ///     Initialises class. No connections are made at init of class, so call `Initialise()` to begin sending and
        ///     recieiving.
        /// </summary>
        public Client(string address, int port, Configuration config = null) {
            TerminateSignaled += Terminate;

            if (!Directory.Exists(Configuration.DefaultResourceDirectory))
                Directory.CreateDirectory(Configuration.DefaultResourceDirectory);

            if (config == null) {
                Configuration.CheckCreateConfig(Configuration.DefaultConfigurationFilePath);
                ClientConfiguration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Configuration.DefaultConfigurationFilePath));
            } else {
                ClientConfiguration = config;
            }

            //MainDatabase = new Database(ClientConfiguration.DatabaseFilePath);
            //MainDatabase.Log += OnLog;

            Connection connection = new Connection(address, port);
            Server = new Server(connection);

            Wrapper = new PluginWrapper<ServerMessagedEventArgs>($@"{AppContext.BaseDirectory}\Plugins", OnInvokedMethod);
            Wrapper.Logged += OnLog;
            Wrapper.TerminateSignaled += OnTerminateSignaled;
            Wrapper.CommandRecieved += Server.Connection.SendDataAsync;
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
            ClientConfiguration?.Dispose();

            _disposed = true;
        }

        #region RUNTIME

        public async Task BeginListenAsync() {
            while (Server.Connection.IsConnected)
                await Server.ListenAsync(this);
        }

        private async Task ChannelMessaged(object source, ServerMessagedEventArgs args) {
            if (string.IsNullOrEmpty(args.Message.Command))
                return;

            if (args.Message.Command.Equals(Commands.PRIVMSG)) {
                if (args.Message.Origin.StartsWith("#"))
                    Server.Channels.Add(new Channel(args.Message.Origin));

                //if (GetUser(args.Message.Realname)?.GetTimeout() ?? false)
                //    return;
            } else if (args.Message.Command.Equals(Commands.ERROR)) {
                return;
            }

            if (args.Message.Nickname.Equals(ClientConfiguration.Nickname) || ClientConfiguration.IgnoreList.Contains(args.Message.Realname))
                return;

            if (args.Message.SplitArgs.Count >= 2 && args.Message.SplitArgs[0].Equals(ClientConfiguration.Nickname.ToLower()))
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();

            try {
                await Wrapper.Host.InvokeAsync(args);
            } catch (Exception ex) {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }

        #endregion

        #region INIT

        public async Task<bool> Initialise() {
            // this subscription done in Initialise() so that event subscriptions
            // to ChannelMessaged from parent classes can be processed before
            // any others.
            //
            // note: this event behaviour is unspecified and may change, so do not
            // rely heavily on it. If it changes in the future I will find another
            // solution.
            //
            Server.ChannelMessaged += ChannelMessaged;

            //await MainDatabase.Initialise();

            await Wrapper.Initialise();

            RegisterMethods();

            await Server.Initialise();
            await OnInitialised(this, new ClassInitialisedEventArgs(this));

            await Server.SendConnectionInfo(ClientConfiguration.Nickname, ClientConfiguration.Realname);
            
            return IsInitialised = Server.Initialised && Wrapper.Initialised;
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
            return ClientConfiguration.ApiKeys[type];
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
