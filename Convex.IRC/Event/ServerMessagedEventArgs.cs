#region USINGS

using System;
using Convex.IRC;
using Convex.IRC.Net;

#endregion

namespace Convex.Event {
    public class ServerMessagedEventArgs : EventArgs {
        public ServerMessagedEventArgs(IIrcClient bot, ServerMessage message) {
            Execute = true;

            Caller = bot;
            Message = message;
        }

        #region MEMBERS

        public bool Execute { get; set; }

        // todo: I don't like this solution
        public IIrcClient Caller { get; }
        public ServerMessage Message { get; }

        #endregion
    }
}