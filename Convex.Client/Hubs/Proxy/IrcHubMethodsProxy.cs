using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHubMethodsProxy {
        private IHubContext<IrcHub> _hubContext;

        public IrcHubMethodsProxy(IHubContext<IrcHub> hubContext) {
            _hubContext = hubContext;
        }

        public async Task BroadcastMessage(string message) {
            await _hubContext.Clients.All.SendAsync(message);
        }

        /// <summary>
        ///     Broadcasts a batch of messages.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, IEnumerable<string> messageBatch, bool isPrepended) {
            if (isPrepended) {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveBroadcastMessageBatchPrepend", messageBatch.Reverse());
            } else {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveBroadcastMessageBatch", messageBatch);
            }
        }
    }
}
