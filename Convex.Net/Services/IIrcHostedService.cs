using System;
using System.Collections.Generic;
using Convex.IRC.Component;
using Convex.IRC.Dependency;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services
{
    public interface IIrcHostedService : IHostedService, IDisposable
    {
        string Address { get; }
        IClient Client { get; }
        List<ServerMessage> Messages { get; }
        int Port { get; }
    }
}