using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Component.Collections;
using Convex.Event;
using Convex.IRC;
using Convex.IRC.Net;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Convex.Util;

namespace Convex.Client.Component {
    public class IrcClientWrapper {
        public IrcClientWrapper(IConfiguration config = null) {
            Channels = new ObservableCollection<Channel>();
            Messages = new SortedList<MessagesIndex, IMessage>();
            _client = new IrcClient(FormatServerMessage, OnInvokedMethod, config);
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

        private string FormatServerMessage(ServerMessage message) {
            return StaticLog.Format(message.Nickname, message.Args);
        }

        public void RegisterMethod(Composition<ServerMessagedEventArgs> args) {
            _client.RegisterMethod(args);
        }

        public int GetMaxIndex() {
            return Messages.Count <= 0 ? 0 : Messages.Keys.Select(index => index.Index).Max();
        }

        public Channel GetChannel(string channelName) {
            return Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));
        }

        public async Task SendMessageAsync(object source, IrcCommandEventArgs args) {
            await _client.Server.Connection.SendDataAsync(source, args);
        }

        #endregion

        #region REGISTRARS

        private void RegisterMethods() {
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step0, IsPing, null, null, Commands.PING));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step1, FixMessageOrigin, null, null, Commands.ALL));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step2, Default, null, null, Commands.ALL));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step3, NamesReply, null, null, Commands.RPL_NAMES));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step3, Join, null, null, Commands.JOIN));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step3, Part, null, null, Commands.PART));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step3, ChannelTopic, null, null, Commands.RPL_TOPIC));
            RegisterMethod(new Composition<ServerMessagedEventArgs>(RegistrarExecutionStep.Step3, NewTopic, null, null, Commands.TOPIC));
        }

        private Task IsPing(ServerMessagedEventArgs args) {
            if (args.Message.Command.Equals(Commands.PING)) {
                args.Execute = false;
            }

            return Task.CompletedTask;
        }

        private Task FixMessageOrigin(ServerMessagedEventArgs args) {
            if (args.Message.Origin.Equals("eve", StringComparison.OrdinalIgnoreCase)) {
                Thread.Sleep(10);
            }

            switch (args.Message.Command) {
                case Commands.RPL_TOPIC:
                    args.Message.Origin = args.Message.SplitArgs[0];
                    break;
                case Commands.RPL_NAMES:
                    // = #channelname :user1 user2 
                    args.Message.Origin = args.Message.SplitArgs[1];
                    break;
                case Commands.NOTICE:
                    if (args.Message.Nickname.Equals("ChanServ", StringComparison.OrdinalIgnoreCase)) {
                        //start: [#channelname]
                        args.Message.Origin = args.Message.SplitArgs[0].Substring(1, args.Message.SplitArgs[0].Length - 2);
                    } else {
                        args.Message.Origin = args.Message.Nickname;
                    }
                    break;
                case Commands.MODE:
                    if (args.Message.Nickname.Equals("ChanServ", StringComparison.OrdinalIgnoreCase)) {
                        break;
                    } else {
                        args.Message.Origin = args.Message.Nickname;
                    }
                    break;
                default:
                    args.Message.Origin = args.Message.Nickname;
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// The default registrar should be registered first, as it will handle all message types.
        /// </summary>
        private Task Default(ServerMessagedEventArgs args) {
#if DEBUG
            Debug.WriteLine(args.Message.RawMessage);
#endif




            if (GetChannel(args.Message.Origin) == null) {
                Channels.Add(new Channel(args.Message.Origin));
            }

            if (args.Message.Command.Equals(Commands.RPL_NAMES)) {
                // = #channelName :user1 user2 user3
                foreach (string user in args.Message.Args.Split(':')[1].Split(' ')) {
                    GetChannel(args.Message.Origin).Inhabitants.Add(new User(user));
                }
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
