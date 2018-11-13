using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Client.Hubs;
using Convex.IRC.Net;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Models.Proxy {
    public class IrcHubContext : IIrcHubContext {
        private IHubContext<IrcHub> _hubContext;

        public IrcHubContext(IHubContext<IrcHub> hubContext) {
            _hubContext = hubContext;
        }

        public async Task BroadcastMessage(string message) {
            await _hubContext.Clients.All.SendAsync("ReceiveBroadcastMessage", message);
        }

        /// <summary>
        ///     Broadcasts a batch of messages.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, IEnumerable<ServerMessage> messageBatch, bool isPrepended) {
            if (isPrepended) {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveBroadcastMessageBatchPrepend", messageBatch);
            } else {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveBroadcastMessageBatch", messageBatch);
            }
        }

        public async Task UpdateMessageInput(string connectionId, string updatedInput) {
            await _hubContext.Clients.Client(connectionId).SendAsync("UpdateMessageInput", updatedInput);
        }
    }
}
