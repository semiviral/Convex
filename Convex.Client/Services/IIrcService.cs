using System;
using System.Threading.Tasks;
using Convex.Client.Component;
using Convex.IRC.Net;

namespace Convex.Client.Services {
    public interface IIrcService : IDisposable {
        IrcClientWrapper IrcClientWrapper { get; }
        IAddress Address { get; }

        Task Initialise();
    }
}