using System;

namespace Convex.IRC.Net.Event {
    public class DisconnectedEventArgs : EventArgs {
        public DisconnectedEventArgs(IConnection connection) {
            Connection = connection;
        }

        #region MEMBERS

        public IConnection Connection { get; }

        #endregion
    }
}