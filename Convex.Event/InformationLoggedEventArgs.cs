using System;

namespace Convex.Event {
    public class InformationLoggedEventArgs : EventArgs {
        public InformationLoggedEventArgs(string information) {
            Information = information;
        }

        #region MEMBERS

        public string Information { get; set; }

        #endregion
    }
}