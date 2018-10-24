using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.IRC.Component;
using Convex.IRC.Dependency;

namespace Convex.Client.Services {
    public interface IIrcService : IDisposable {
        string Address { get; }
        IClient Client { get; }
        SortedList<int, ServerMessage> Messages { get; }
        int Port { get; }
    }
}