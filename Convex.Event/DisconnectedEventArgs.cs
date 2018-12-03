using System;

namespace Convex.Event {
    public class DisconnectedEventArgs : EventArgs {
        public DisconnectedEventArgs(object connection, string information) {
            Connection = connection;
            Information = information;
        }

        #region MEMBERS

        public object Connection { get; }
        public string Information { get; }

        #endregion
    }
}