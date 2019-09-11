#region

using System;

#endregion

namespace Convex.Event
{
    public class OperationTerminatedEventArgs : EventArgs
    {
        public OperationTerminatedEventArgs(object terminatedObject, string information)
        {
            TerminatedObject = terminatedObject;
            Information = information;
        }

        #region MEMBERS

        public object TerminatedObject { get; }
        public string Information { get; set; }

        #endregion
    }
}
