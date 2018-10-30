using System;
using Convex.IRC.Component.Net;

namespace Convex.Irc.Component.Net.Event {
    public class DisconnectedEventArgs : EventArgs {
        public DisconnectedEventArgs(IConnection connection) {
            Connection = connection;
        }

        #region MEMBERS

        public IConnection Connection { get; }

        #endregion
    }
}