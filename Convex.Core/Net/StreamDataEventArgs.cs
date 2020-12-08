#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class StreamDataEventArgs : EventArgs
    {
        #region MEMBERS

        public string Data { get; set; }

        #endregion

        public StreamDataEventArgs(string data)
        {
            Data = data;
        }
    }
}