using System;

namespace Convex.Event {
    public class IrcCommandReceivedEventArgs : EventArgs {
        public IrcCommandReceivedEventArgs(string command, string arguments) {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString() {
            return $"{Command} {Arguments}";
        }

        #region MEMBERS

        public string Command { get; set; }
        public string Arguments { get; set; }

        #endregion
    }
}