#region

using System;

#endregion

namespace Convex.Event
{
    public class DatabaseQueriedEventArgs : EventArgs
    {
        public DatabaseQueriedEventArgs(string query)
        {
            Query = query;
        }

        #region MEMBERS

        public string Query { get; set; }

        #endregion
    }
}
