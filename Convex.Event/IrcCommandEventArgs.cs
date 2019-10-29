#region

using System;

#endregion

namespace Convex.Event
{
    public class IrcCommandEventArgs : EventArgs
    {
        public IrcCommandEventArgs(string command, string arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString() => $"{Command} {string.Join(' ', Arguments)}";

        #region MEMBERS

        public string Command { get; set; }
        public string Arguments { get; set; }

        #endregion
    }
}
