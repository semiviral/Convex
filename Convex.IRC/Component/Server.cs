#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Component.Event;
using Convex.IRC.Component.Net;
using Convex.IRC.Component.Reference;
using Convex.IRC.Dependency;

#endregion

namespace Convex.IRC.Component {
    public class Server : IDisposable {
        public Server() {
            Connection = new Connection();
            Channels = new ObservableCollection<Channel>();
            Channels.CollectionChanged += async (sender, args) => await ChannelCollectionChanged(sender, args);
        }

        #region INTERFACE IMPLEMENTATION

        public void Dispose() {
            Connection?.Dispose();

            Identified = false;
            Initialised = false;
        }

        #endregion

        #region RUNTIME

        /// <summary>
        ///     Waits for response from server and processes response
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Use this method to begin listening cycle.
        /// </remarks>
        internal async Task ListenAsync(IClient caller) {
            string rawData = await Connection.ListenAsync();

            if (string.IsNullOrEmpty(rawData) || await CheckPing(rawData))
                return;

            await OnChannelMessaged(this, new ServerMessagedEventArgs(caller, new ServerMessage(rawData)));
        }

        #endregion

        #region MEMBERS

        public IConnection Connection { get; }

        public bool Identified { get; set; }
        public bool Initialised { get; private set; }
        public bool Executing => Connection.Executing;

        public ObservableCollection<Channel> Channels { get; }

        public List<string> AllUsers => Channels.SelectMany(e => e.Inhabitants).ToList();

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ServerMessagedEventArgs> ServerMessaged;

        private async Task OnChannelMessaged(object sender, ServerMessagedEventArgs args) {
            if (ServerMessaged == null)
                return;

            await ServerMessaged.Invoke(sender, args);
        }

        private async Task ChannelCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) {
            foreach (object newItem in args.NewItems) {
                if (!(newItem is Channel))
                    continue;

                switch (args.Action) {
                    case NotifyCollectionChangedAction.Add:
                        if (Channels.Select(channel => channel.Name.Equals(((Channel) newItem).Name)).Count() > 1)
                            break;

                        await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.JOIN, ((Channel) newItem).Name));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (!Channels.Select(channel => channel.Name).Contains(((Channel) newItem).Name))
                            break;

                        await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.PART, ((Channel) newItem).Name));
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
            }
        }

        #endregion

        #region INIT

        public async Task Initialise(string address, int port) {
            await Connection.Initialise(address, port);

            Initialised = Connection.IsInitialised;
        }

        /// <summary>
        ///     sends client info to the server
        /// </summary>
        public async Task SendConnectionInfo(string nickname, string realname) {
            await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.USER, $"{nickname} 0 * {realname}"));
            await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.NICK, nickname));

            Identified = true;
        }

        #endregion

        #region METHODS

        public Channel GetChannel(string name) {
            return Channels.SingleOrDefault(channel => channel.Name.Equals(name));
        }

        public bool RemoveChannel(string name) {
            return Channels.Remove(GetChannel(name));
        }

        /// <summary>
        ///     Check whether the data received is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private async Task<bool> CheckPing(string rawData) {
            if (!rawData.StartsWith(Commands.PING))
                return false;

            await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.PONG, rawData.Remove(0, 5))); // removes 'PING ' from string
            return true;
        }

        #endregion
    }
}