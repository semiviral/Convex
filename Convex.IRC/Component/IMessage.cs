using System;

namespace Convex.IRC.Net {
    public interface IMessage {
        string RawMessage { get; }
        string Formatted { get; }
        string Origin { get; }
        string Nickname { get; }
        DateTime Timestamp { get; }
    }
}