using System;
using Convex.Client.Model;

namespace Convex.Client.Services {
    public interface IIrcService : IDisposable {
        string Address { get; }
        IIrcClientWrapper IrcClientWrapper { get; }
        int Port { get; }
    }
}