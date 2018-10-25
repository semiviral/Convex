using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.Event;
using Convex.IRC.Component.Event;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Hubs {
    public class IrcHubProxyService : IHostedService, IIrcHubProxyService {
        public IrcHubProxyService(IIrcService ircService, IrcHubMethodsProxy ircHubMethodsProxy) {
            _ircService = ircService;
            _ircService.Client.Server.ServerMessaged += OnIrcServiceServerMessaged;
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

        #region EVENT

        private async Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            await _ircHubMethodsProxy.BroadcastMessage(args.Message.RawMessage);
        }

        #endregion

        #region METHODS

        public async Task SendMessage(string rawMessage) {
            await _ircService.Client.Server.Connection.SendDataAsync(this, new IrcCommandEventArgs(string.Empty, rawMessage));
        }

        /// <summary>
        ///     Broadcasts a batch of messages.
        ///     The client's first index is the maximum index, decrementing from there. So the most recent index is the first index the list.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, int startIndex, int endIndex, bool isPrepend) {
            if (_ircService.Messages.Count <= 0) {
                return;
            }

            Tuple<int, int> reversedIndexValues = ReverseIndexValues(startIndex, endIndex);
            startIndex = reversedIndexValues.Item1;
            endIndex = reversedIndexValues.Item2;

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

        private Tuple<int, int> ReverseIndexValues(int startIndex, int endIndex) {
            int maximumMessagesKey = _ircService.Messages.Keys.Max();

            int tempStartIndex = startIndex;
            int tempEndIndex = endIndex;

            tempStartIndex = maximumMessagesKey - endIndex;
            tempEndIndex += startIndex;

            if (tempStartIndex < 0) {
                tempStartIndex = 0;
            }

            if (tempEndIndex > maximumMessagesKey) {
                tempEndIndex = maximumMessagesKey;
            }

            return new Tuple<int, int>(tempStartIndex, tempEndIndex);
        }

        #endregion
    }
}
