using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC;
using Convex.IRC.Net;
using Convex.Plugin.Registrar;
using Convex.Util;

namespace Convex.Client.Model {
    public class IrcClientWrapper : IIrcClientWrapper {
        public IrcClientWrapper(IConfiguration config = null) {
            Channels = new List<Channel>();
            Messages = new SortedList<Tuple<int, DateTime>, ServerMessage>();
            _baseClient = new IrcClient(config);

            RegisterMethods();
        }

        #region MEMBERS

        public bool IsInitialised => _baseClient.IsInitialised;

        public List<Channel> Channels { get; }
        public SortedList<Tuple<int, DateTime>, ServerMessage> Messages { get; }

        private IrcClient _baseClient;

        #endregion

        #region INIT

        public async Task Initialise(string address, int port) {
            await _baseClient.Initialise(address, port);
        }

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync() {
            await _baseClient.BeginListenAsync();
        }

        #endregion

        #region METHODS

        public void RegisterMethod(MethodRegistrar<ServerMessagedEventArgs> args) {
            _baseClient.RegisterMethod(args);
        }

        public int GetMaxIndex() {
            return Messages.Count <= 0 ? 0 : Messages.Keys.Select(tup => tup.Item1).Max();
        }

        public Channel GetChannel(string channelName) {
            return Channels.Single(channel => channel.Name.Equals(channelName));
        }

        public async Task SendMessageAsync(object sender, IrcCommandEventArgs args) {
            await _baseClient.Server.Connection.SendDataAsync(sender, args);
        }

        #endregion

        #region REGISTRARS

        private void RegisterMethods() {
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Default, null, Commands.ALL, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NamesReply, null, Commands.NAMES_REPLY, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Join, null, Commands.JOIN, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Part, null, Commands.PART, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(ChannelTopic, null, Commands.CHANNEL_TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NewTopic, null, Commands.TOPIC, null));
        }

        /// <summary>
        /// The default registrar should be registered first, as it will handle all message types.
        /// </summary>
        private Task Default(ServerMessagedEventArgs args) {
            Messages.Add(new Tuple<int, DateTime>(GetMaxIndex(), args.Message.Timestamp), args.Message);

            return Task.CompletedTask;
        }

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, you are the only one in the channel
            if (e.Message.SplitArgs.Count < 4) {
                return Task.CompletedTask;
            }

            foreach (string user in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = GetChannel(channelName);

                if (currentChannel == null || currentChannel == default(Channel)) {
                    continue;
                }

                User newUser = new User(user);

                GetChannel(channelName).Inhabitants.Add(newUser);
            }

            return Task.CompletedTask;
        }

        private Task Nick(ServerMessagedEventArgs e) {
            //await _ba.OnQuery(this, new DatabaseQueriedEventArgs($"UPDATE users SET nickname='{e.Message.Origin}' WHERE realname='{e.Message.Realname}'"));

            return Task.CompletedTask;
        }

        private Task Join(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin)?.Inhabitants.Add(new User(e.Message.Nickname));

            return Task.CompletedTask;
        }

        private Task Part(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin)?.Inhabitants.RemoveAll(x => x.Nickname.Equals(e.Message.Nickname));

            return Task.CompletedTask;
        }

        private Task ChannelTopic(ServerMessagedEventArgs e) {
            GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);

            return Task.CompletedTask;
        }

        private Task NewTopic(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin).Topic = e.Message.Args;

            return Task.CompletedTask;
        }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public void Dispose() {
            _baseClient.Dispose();
        }

        #endregion
    }
}
