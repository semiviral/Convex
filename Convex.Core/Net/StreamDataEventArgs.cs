#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class StreamDataEventArgs : EventArgs
    {
        public StreamDataEventArgs(string data) => Data = data;

        #region MEMBERS

        public string Data { get; set; }

        #endregion
    }
}
