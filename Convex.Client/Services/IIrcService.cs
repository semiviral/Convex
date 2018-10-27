using System;
using System.Collections.Generic;
using Convex.IRC.Component;
using Convex.IRC.Dependency;

namespace Convex.Client.Services {
    public interface IIrcService : IDisposable {
        string Address { get; }
        IClient Client { get; }
        SortedList<DateTime, ServerMessage> Messages { get; }
        int Port { get; }
    }
}