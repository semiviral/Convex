using System;

namespace Convex.IRC.Net {
    public interface IMessage {
        DateTime Timestamp { get; }
        string RawMessage { get; }
        string Formatted { get; }
        string Origin { get; }
        string Nickname { get; }
    }
}