#region

using System;

#endregion

namespace Convex.Core.Events
{
    public class DatabaseQueriedEventArgs : EventArgs
    {
        #region MEMBERS

        public string Query { get; set; }

        #endregion

        public DatabaseQueriedEventArgs(string query)
        {
            Query = query;
        }
    }
}