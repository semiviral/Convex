using System;

namespace Convex.Event {
    public class OperationTerminatedEventArgs : EventArgs {
        #region MEMBERS

        public object TerminatedObject { get; }
        public string Information { get; set; }

        #endregion

        public OperationTerminatedEventArgs(object terminatedObject, string information) {
            TerminatedObject = terminatedObject;
            Information = information;
        }
    }
}
