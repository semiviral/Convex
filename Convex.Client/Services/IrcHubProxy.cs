using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Hubs.Proxy;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public class IrcHubProxy : IIrcHubProxy {
        public IrcHubProxy(IIrcService ircService, IrcHubMethodsProxy ircHubMethodsProxy) {
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
            await _ircHubMethodsProxy.BroadcastMessage(FormatServerMessage(args.Message));
        }

        #endregion

        #region CLIENT TO SERVER METHODS

        public async Task SendMessage(string rawMessage) {
            await _ircHubMethodsProxy.BroadcastMessage(StaticLog.FormatLogAsOutput(_ircService.Client.Config.Nickname, rawMessage));
            await _ircService.Client.Server.Connection.SendDataAsync(this, new IrcCommandEventArgs(string.Empty, rawMessage));
        }

        #endregion

        #region SERVER TO CLIENT METHODS

        /// <summary>
        ///     Broadcasts a batch of messages.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, DateTime startIndex, DateTime endIndex) {
            if (_ircService.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= DateTime.Now) {
                return;
            }
            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.Messages.Where(kvp => IsDateBetween(kvp.Key.Item2, startIndex, endIndex)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex) {
            if (_ircService.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= 0) {
                return;
            }

            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.Messages.Where(kvp => IsIntBetween(kvp.Key.Item1, startIndex, endIndex)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<ServerMessage> messageBatch) {
            if (isPrepend) {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(message => FormatServerMessage(message)), true);
            } else {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(message => FormatServerMessage(message)), false);
            }
        }

        #endregion

        #region METHODS

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

        private bool IsIntBetween(int value, int int1, int int2) {
            return value >= int1 && value <= int2;
        }

        private string FormatServerMessage(ServerMessage message) {
            return StaticLog.FormatLogAsOutput(message);
        }

        #endregion
    }
}
