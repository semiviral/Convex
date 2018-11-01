using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convex.Client.Models.Proxy {
    public interface IIrcHubMethodsProxy {
        Task BroadcastMessage(string message);
        Task BroadcastMessageBatch(string connectionId, IEnumerable<string> messageBatch, bool isPrepended);
        Task UpdateMessageInput(string connectionId, string updatedInput);
        Task AddChannel(string channelName);
        Task RemoveChannel(string channelName);
    }
}