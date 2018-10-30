using System;

namespace Convex.IRC.Net.Event {
    public class StreamFlushedEventArgs : EventArgs {
        public StreamFlushedEventArgs(string information) {
            Information = information;
        }

        #region MEMBERS

        public string Information { get; set; }

        #endregion
    }
}