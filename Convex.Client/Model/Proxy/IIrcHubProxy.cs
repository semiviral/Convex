using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Net;

namespace Convex.Client.Proxy {
    public interface IIrcHubProxy {
        Task UpdateSelectedChannel(string channelName);
        Task GetMessageBatchByChannel(string connectionId, string channelName, int startIndex, int endIndex);
        Task SendMessage(string rawMessage);
        
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, DateTime startIndex, DateTime endIndex);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<ServerMessage> messageBatch);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex);
        Task UpdateMessageInput(string connectionId, bool previousMessage);

        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}