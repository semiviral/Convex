#region

using System;

#endregion

namespace Convex.Core.Component
{
    public interface IMessage
    {
        DateTime Timestamp { get; }
        string RawMessage { get; }
        string Formatted { get; }
        string Origin { get; }
        string Nickname { get; }
    }
}
