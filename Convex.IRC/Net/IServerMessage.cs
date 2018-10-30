using System;
using System.Collections.Generic;

namespace Convex.IRC.Net {
    public interface IServerMessage {
        string Args { get; }
        string Command { get; }
        string Hostname { get; }
        string InputCommand { get; set; }
        bool IsIrCv3Message { get; }
        string Nickname { get; }
        string Origin { get; }
        string RawMessage { get; }
        string Realname { get; }
        List<string> SplitArgs { get; }
        DateTime Timestamp { get; }

        void Parse();
        string ToString();
    }
}