using System;

namespace Convex.Event {
    public class DatabaseQueriedEventArgs : EventArgs {
        #region MEMBERS

        public string Query { get; set; }

        #endregion

        public DatabaseQueriedEventArgs(string query) {
            Query = query;
        }
    }
}
