#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class DisconnectedEventArgs : EventArgs
    {
        public DisconnectedEventArgs(object connection, string information)
        {
            Connection = connection;
            Information = information;
        }

        #region MEMBERS

        public object Connection { get; }
        public string Information { get; }

        #endregion
    }
}