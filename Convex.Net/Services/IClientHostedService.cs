using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Component;
using Convex.IRC.Dependency;

namespace Convex.Client.Services
{
    public interface IClientHostedService
    {
        string Address { get; }
        IClient Client { get; }
        int Port { get; }

        IEnumerable<ServerMessage> GetAllMessages();
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}