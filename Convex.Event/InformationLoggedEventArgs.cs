using System;

namespace Convex.Event {
    public class InformationLoggedEventArgs : EventArgs {
        #region MEMBERS

        public string Information { get; set; }

        #endregion

        public InformationLoggedEventArgs(string information) {
            Information = information;
        }
    }
}
