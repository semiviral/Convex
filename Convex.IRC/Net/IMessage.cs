using System;
using System.Collections.Generic;

namespace Convex.IRC.Net {
    public interface IMessage {
        string Args { get; }
        string Formatted { get; }
        string Nickname { get; }
        string Origin { get; }
        string RawMessage { get; }
        List<string> SplitArgs { get; }
        DateTime Timestamp { get; }

        void Parse();
        string ToString();
    }
}