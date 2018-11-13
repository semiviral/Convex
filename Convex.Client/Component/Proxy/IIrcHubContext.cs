﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Client.Component;
using Convex.IRC.Net;

namespace Convex.Client.Models.Proxy {
    public interface IIrcHubContext {
        Task BroadcastMessage(string message);
        Task BroadcastMessageBatch(string connectionId, IEnumerable<ServerMessage> messageBatch, bool isPrepended);
        Task AddChannel(Channel channel);
        Task RemoveChannel(Channel channel);
        Task UpdateMessageInput(string connectionId, string updatedInput);
    }
}