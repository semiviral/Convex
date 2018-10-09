#region usings

using System;
using Convex.IRC.Dependency;

#endregion

namespace Convex.IRC.Component.Event {
    public class ServerMessagedEventArgs : EventArgs {
        #region MEMBERS

        // todo: I don't like this solution
        public IClient Caller { get; }
        public ServerMessage Message { get; }

        #endregion

        public ServerMessagedEventArgs(IClient bot, ServerMessage message) {
            Caller = bot;
            Message = message;
        }
    }
}
