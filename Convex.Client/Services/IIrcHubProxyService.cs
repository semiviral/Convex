﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public interface IIrcHubProxyService : IHostedService {
        Task SendMessage(string rawMessage);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, DateTime startIndex, DateTime endIndex);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex);
        Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<string> messageBatch);

    }
}