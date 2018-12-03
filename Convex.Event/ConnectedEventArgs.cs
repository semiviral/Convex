using System;

namespace Convex.Event {
    public class ConnectedEventArgs : EventArgs {
        public ConnectedEventArgs(object connection, string information) {
            Connection = connection;
            Information = information;
        }

        #region MEMBERS

        public object Connection { get; }
        public string Information { get; }

        #endregion
    }
}