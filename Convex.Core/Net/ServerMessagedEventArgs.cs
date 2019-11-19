#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class ServerMessagedEventArgs : EventArgs
    {
        public ServerMessagedEventArgs(IInitializedClient bot, ServerMessage message)
        {
            Execute = true;
            Display = true;

            Caller = bot;
            Message = message;
        }

        #region MEMBERS

        public bool Execute { get; set; }
        public bool Display { get; set; }

        // todo: I don't like this solution
        public IInitializedClient Caller { get; }
        public ServerMessage Message { get; }

        #endregion
    }
}