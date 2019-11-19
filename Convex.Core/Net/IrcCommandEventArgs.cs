#region

using System;

#endregion

namespace Convex.Core.Net
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string command, string arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString()
        {
            return $"{Command} {string.Join(' ', Arguments)}";
        }

        #region MEMBERS

        public string Command { get; set; }
        public string Arguments { get; set; }

        #endregion
    }
}