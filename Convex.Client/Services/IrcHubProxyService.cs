using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Hubs.Proxy;
using Convex.Event;
using Convex.IRC.Component.Event;
using Microsoft.Extensions.Hosting;
using Convex.IRC.Component;

namespace Convex.Client.Services {
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
        public async Task BroadcastMessageBatch(string connectionId, DateTime startIndex, DateTime endIndex, bool isPrepend) {
            if (_ircService.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= DateTime.Now) {
                return;
            }

            IEnumerable<string> messageBatch = _ircService.Messages.Where(kvp => IsDateBetween(kvp.Key, startIndex, endIndex)).Select(kvp => kvp.Value.RawMessage);

            if (isPrepend) {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch, true);
            } else {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch, false);
            }
        }

        private IEnumerable<DateTime> GetDatesBetween(IEnumerable<DateTime> dateList, DateTime startDate, DateTime endDate) {
            foreach (DateTime date in dateList) {
                if (IsDateBetween(date, startDate, endDate)) {
                    yield return date;
                }
            }
        }

        private bool IsDateBetween(DateTime date, DateTime startDate, DateTime endDate) {
            return date >= startDate && date <= endDate;
        }

        #endregion
    }
}
