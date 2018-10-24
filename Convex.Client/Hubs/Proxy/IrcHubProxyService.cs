using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.Event;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Hubs {
    public class IrcHubProxyService : IHostedService, IIrcHubProxyService {
        public IrcHubProxyService(IIrcService ircService, IrcHubMethodsProxy ircHubMethodsProxy) {
            _ircService = ircService;
            _ircHubMethodsProxy = ircHubMethodsProxy;
        }

        #region MEMBERS

        private IIrcService _ircService;
        private IrcHubMethodsProxy _ircHubMethodsProxy;

        #endregion

        #region INTERFACE IMPLEMENTATION

        public Task StartAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        #endregion

        #region METHODS

        public async Task SendMessage(string rawMessage) {
            await _ircService.Client.Server.Connection.SendDataAsync(this, new IrcCommandEventArgs(string.Empty, rawMessage));
        }

        /// <summary>
        ///     Broadcasts a batch of messages.
        ///     The client's first index is the maximum index, decrementing from there. So the most recent index is the first.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, int startIndex, int endIndex, bool isPrepend) {
            await WaitForIrcServiceInitialised();

            if (startIndex <= endIndex || endIndex < 0) {
                return;
            }

            IEnumerable<string> messageBatch = _ircService.Messages.Skip(startIndex).Take(endIndex - startIndex + 1).Select(message => message.Value.RawMessage).Reverse();

            if (isPrepend) {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch, true);
            } else {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch, false);
            }
        }

        private async Task WaitForIrcServiceInitialised() {
            while (!_ircService.Client.IsInitialised) {
                await Task.Delay(200);
            }
        }

        private void TransformIndexValues(int startIndex, int endIndex) {
            int maximumMessagesKey = _ircService.Messages.Keys.Max();

            startIndex = maximumMessagesKey - startIndex;
            endIndex = maximumMessagesKey - endIndex;
        }

        #endregion
    }
}
