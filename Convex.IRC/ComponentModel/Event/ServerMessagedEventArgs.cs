#region usings

using System;
using Convex.IRC.Models;

#endregion

namespace Convex.IRC.ComponentModel.Event {
    public class ServerMessagedEventArgs : EventArgs {
        #region MEMBERS

        // todo: I don't like this solution
        public Client Caller { get; }
        public ServerMessage Message { get; }

        #endregion

        public ServerMessagedEventArgs(Client bot, ServerMessage message) {
            Caller = bot;
            Message = message;
        }
    }
}
