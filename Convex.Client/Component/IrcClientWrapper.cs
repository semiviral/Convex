using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Convex.Client.Component.Collections;
using Convex.Event;
using Convex.IRC;
using Convex.IRC.Net;
using Convex.Plugin.Registrar;
using Convex.Util;

namespace Convex.Client.Component {
    public class IrcClientWrapper {
        public IrcClientWrapper(IConfiguration config = null) {
            Channels = new ObservableCollection<Channel>();
            Messages = new SortedList<MessagesIndex, IMessage>();
            _client = new IrcClient(FormatServerMessage, config);
            firstMessageAdded = false;

            RegisterMethods();
        }

        #region MEMBERS

        public bool IsInitialised => _client.IsInitialised;

        public ObservableCollection<Channel> Channels { get; }
        public SortedList<MessagesIndex, IMessage> Messages { get; }

        private IrcClient _client;

        private bool firstMessageAdded;

        #endregion

        #region INIT

        public async Task Initialise(IAddress address) {
            await _client.Initialise(address);
        }

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync() {
            await _client.BeginListenAsync();
        }

        #endregion

        #region METHODS

        private bool ParseMessageFlag(ServerMessage message) {
            switch (message.Command) {
                case Commands.PING:
                    return false;
            }

            return true;
        }

        private void FixMessageOrigin(ServerMessage message) {
            switch (message.Command) {
                case Commands.PRIVMSG:
                    break;
                case Commands.RPL_TOPIC:
                    message.Origin = message.SplitArgs[0].Substring(2, message.SplitArgs[0].Length - 1);
                    break;
                default:
                    message.Origin = message.Nickname;
                    break;
            }
        }

        private string FormatServerMessage(ServerMessage message) {
            return StaticLog.Format(message.Nickname, message.Args);
        }

        public void RegisterMethod(MethodRegistrar<ServerMessagedEventArgs> args) {
            _client.RegisterMethod(args);
        }

        public int GetMaxIndex() {
            return Messages.Count <= 0 ? 0 : Messages.Keys.Select(index => index.Index).Max();
        }

        public Channel GetChannel(string channelName) {
            return Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));
        }

        public async Task SendMessageAsync(object sender, IrcCommandEventArgs args) {
            await _client.Server.Connection.SendDataAsync(sender, args);
        }

        #endregion

        #region REGISTRARS

        private void RegisterMethods() {
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Default, null, Commands.ALL, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NamesReply, null, Commands.RPL_NAMES, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Join, null, Commands.JOIN, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(Part, null, Commands.PART, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(ChannelTopic, null, Commands.RPL_TOPIC, null));
            RegisterMethod(new MethodRegistrar<ServerMessagedEventArgs>(NewTopic, null, Commands.TOPIC, null));
        }

        /// <summary>
        /// The default registrar should be registered first, as it will handle all message types.
        /// </summary>
        private Task Default(ServerMessagedEventArgs args) {
#if DEBUG
            Debug.WriteLine(args.Message.RawMessage);
#endif

            if (ParseMessageFlag(args.Message)) {
                FixMessageOrigin(args.Message);
            } else { return Task.CompletedTask; }

            if (GetChannel(args.Message.Origin) == null) {
                Channels.Add(new Channel(args.Message.Origin));
            }

            if (firstMessageAdded) {
                Messages.Add(new MessagesIndex(GetMaxIndex() + 1, args.Message.Timestamp, args.Message.Origin, true), args.Message);
            } else {
                firstMessageAdded = true;

                Messages.Add(new MessagesIndex(0, args.Message.Timestamp, args.Message.Origin, true), args.Message);
            }

            return Task.CompletedTask;
        }

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            if (GetChannel(e.Message.SplitArgs[1]) == null) {
                Channels.Add(new Channel(channelName));
            }

            // * SplitArgs [2] is always your nickname
            // in this case, you are the only one in the channel
            if (e.Message.SplitArgs.Count < 4) {
                return Task.CompletedTask;
            }

            foreach (string user in e.Message.SplitArgs[3].Split(' ')) {
                User newUser = new User(user);

                GetChannel(e.Message.SplitArgs[1]).Inhabitants.Add(newUser);
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
            GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.SplitArgs[1].Substring(1);

            return Task.CompletedTask;
        }

        private Task NewTopic(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin).Topic = e.Message.Args;

            return Task.CompletedTask;
        }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public void Dispose() {
            _client.Dispose();
        }

        #endregion
    }
}
