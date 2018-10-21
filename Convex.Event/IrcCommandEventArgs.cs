﻿using System;

namespace Convex.Event {
    public class IrcCommandEventArgs : EventArgs {
        public IrcCommandEventArgs(string command, params string[] arguments) {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString() {
            return $"{Command} {string.Join(' ', Arguments)}";
        }

        #region MEMBERS

        public string Command { get; set; }
        public string[] Arguments { get; set; }

        #endregion
    }
}