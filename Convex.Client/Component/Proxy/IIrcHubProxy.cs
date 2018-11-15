using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.IRC.Net;

namespace Convex.Client.Proxy {
    public interface IIrcHubProxy {
        Task RequestBroadcastChannels(string connectionId);
        Task SendMessage(string rawMessage);

        Task BroadcastMessageBatch(string connectionId, bool isPrepend, string channelName, DateTime startDate, DateTime endDate);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, string channelName, int startIndex, int length);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<IMessage> messageBatch);
        Task UpdateMessageInput(string connectionId, bool previousMessage);
    }
}