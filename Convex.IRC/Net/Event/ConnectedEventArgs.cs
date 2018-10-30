using System;

namespace Convex.IRC.Net.Event {
    public class ConnectedEventArgs : EventArgs {
        public ConnectedEventArgs(IConnection connection) {
            Connection = connection;
        }

        #region MEMBERS

        public IConnection Connection { get; }

        #endregion
    }
}