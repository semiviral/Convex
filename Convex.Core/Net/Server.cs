#region

using System;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.Core.Net
{
    public class Server : IDisposable
    {
        public Server()
        {
            Connection = new Connection();
        }

        #region INTERFACE IMPLEMENTATION

        public void Dispose()
        {
            Connection?.Dispose();

            Identified = false;
            IsInitialized = false;
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
        public async Task ListenAsync(IClient caller)
        {
            string rawData = await Connection.ListenAsync();

            if (string.IsNullOrEmpty(rawData) || await CheckIsPing(rawData))
            {
                return;
            }

            await OnChannelMessaged(this, new ServerMessagedEventArgs(caller, new ServerMessage(rawData)));
        }

        #endregion

        #region MEMBERS

        public Connection Connection { get; }
        public bool Identified { get; set; }
        public bool IsInitialized { get; private set; }
        public bool Executing => Connection.Executing;

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ServerMessagedEventArgs> MessageReceived;

        private async Task OnChannelMessaged(object source, ServerMessagedEventArgs args)
        {
            if (MessageReceived == null)
            {
                return;
            }

            await MessageReceived.Invoke(source, args);
        }

        #endregion

        #region INIT

        public async Task Initialize(IAddress address)
        {
            await Connection.Initialize(address);

            IsInitialized = Connection.IsInitialized;
        }

        /// <summary>
        ///     sends client info to the server
        /// </summary>
        public async Task SendIdentityInfo(string nickname, string realname)
        {
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
        private async Task<bool> CheckIsPing(string rawData)
        {
            if (!rawData.StartsWith(Commands.PING))
            {
                return false;
            }

            await Connection.SendDataAsync(this,
                new IrcCommandEventArgs(Commands.PONG, rawData.Remove(0, 5))); // removes 'PING ' from string
            return true;
        }

        #endregion
    }
}
