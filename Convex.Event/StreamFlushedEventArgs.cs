using System;

namespace Convex.Event {
    public class StreamFlushedEventArgs : EventArgs {
        #region MEMBERS

        public string Information { get; set; }

        #endregion

        public StreamFlushedEventArgs(string information) {
            Information = information;
        }
    }
}
