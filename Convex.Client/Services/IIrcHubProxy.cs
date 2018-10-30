using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Net;

namespace Convex.Client.Services {
    public interface IIrcHubProxy {
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, DateTime startIndex, DateTime endIndex);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<ServerMessage> messageBatch);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex);
        Task SendMessage(string rawMessage);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}