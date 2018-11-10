using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.IRC.Net;

namespace Convex.Client.Models.Proxy {
    public interface IIrcHubMethodsProxy {
        Task BroadcastMessage(string message);
        Task BroadcastMessageBatch(string connectionId, IEnumerable<ServerMessage> messageBatch, bool isPrepended);
        Task UpdateMessageInput(string connectionId, string updatedInput);
    }
}