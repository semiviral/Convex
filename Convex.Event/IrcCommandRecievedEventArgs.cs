using System;

namespace Convex.Event {
    public class IrcCommandRecievedEventArgs : EventArgs {
        #region MEMBERS

        public string Command { get; set; }
        public string Arguments { get; set; }

        #endregion

        public IrcCommandRecievedEventArgs(string command, string arguments) {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString() {
            return $"{Command} {Arguments}";
        }
    }
}
