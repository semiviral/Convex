#region USINGS

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Util;

#endregion

namespace Convex.IRC.Net {
    public class Server : IDisposable, IServer {
        public Server(Func<ServerMessage, string> formatter) {
            Connection = new Connection();
            _formatter = formatter;
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
        public async Task ListenAsync(IIrcClient caller) {
            string rawData = await Connection.ListenAsync();

            if (string.IsNullOrEmpty(rawData) || await CheckPing(rawData)) {
                return;
            }

            await OnChannelMessaged(this, new ServerMessagedEventArgs(caller, new ServerMessage(rawData, ref _formatter)));
        }

        #endregion

        #region MEMBERS

        private Func<ServerMessage, string> _formatter;

        public IConnection Connection { get; }
        public bool Identified { get; set; }
        public bool Initialised { get; private set; }
        public bool Executing => Connection.Executing;

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ServerMessagedEventArgs> ServerMessaged;

        private async Task OnChannelMessaged(object sender, ServerMessagedEventArgs args) {
            if (ServerMessaged == null) {
                return;
            }

            await ServerMessaged.Invoke(sender, args);
        }

        #endregion

        #region INIT

        public async Task Initialise(IAddress address) {
            await Connection.Initialise(address);

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

        /// <summary>
        ///     Check whether the data received is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private async Task<bool> CheckPing(string rawData) {
            if (!rawData.StartsWith(Commands.PING)) {
                return false;
            }

            await Connection.SendDataAsync(this, new IrcCommandEventArgs(Commands.PONG, rawData.Remove(0, 5))); // removes 'PING ' from string
            return true;
        }

        #endregion
    }
}