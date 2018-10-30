using System;
using System.Threading.Tasks;
using Convex.Client.Model;
using Convex.IRC.Net;

namespace Convex.Client.Services {
    public interface IIrcService : IDisposable {
        IIrcClientWrapper IrcClientWrapper { get; }
        IAddress Address { get; }

        Task Initialise();
    }
}