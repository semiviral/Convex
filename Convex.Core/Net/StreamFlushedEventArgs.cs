#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class StreamFlushedEventArgs : EventArgs
    {
        public StreamFlushedEventArgs(string information) => Information = information;

        #region MEMBERS

        public string Information { get; set; }

        #endregion
    }
}
