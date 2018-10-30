using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Models.Proxy;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.IRC.Component.Reference;

namespace Convex.Client.Services {
    public class IrcHubProxy : IIrcHubProxy {
        public IrcHubProxy(IIrcService ircService, IrcHubMethodsProxy ircHubMethodsProxy) {
            _ircService = ircService;
            _ircHubMethodsProxy = ircHubMethodsProxy;

            _ircService.IrcClientWrapper.RegisterMethod(new Plugin.Registrar.MethodRegistrar<ServerMessagedEventArgs>(OnIrcServiceServerMessaged, null, Commands.ALL, null));
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

        #region REGISTRARS

        private async Task OnIrcServiceServerMessaged(ServerMessagedEventArgs args) {
            await _ircHubMethodsProxy.BroadcastMessage(FormatServerMessage(args.Message));
        }

        #endregion

        #region CLIENT TO SERVER METHODS

        public async Task SendMessage(string rawMessage) {
            if (string.IsNullOrWhiteSpace(rawMessage)) {
                return;
            }

            await _ircHubMethodsProxy.BroadcastMessage(StaticLog.FormatLogAsOutput(Program.Config.Nickname, rawMessage));
            await _ircService.IrcClientWrapper.SendMessageAsync(this, ConvertToCommandArgs(rawMessage));
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
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= DateTime.Now) {
                return;
            }
            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.IrcClientWrapper.Messages.Where(kvp => IsDateBetween(kvp.Key.Item2, startIndex, endIndex)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex) {
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= 0) {
                return;
            }

            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.IrcClientWrapper.Messages.Where(kvp => IsIntBetween(kvp.Key.Item1, startIndex, endIndex)).Select(kvp => kvp.Value));
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

        private IrcCommandEventArgs ConvertToCommandArgs(string rawMessage) {
            int firstSpaceIndex = rawMessage.IndexOf(' ') == -1 ? 0 : rawMessage.IndexOf(' ');
            string command = rawMessage.Substring(0, firstSpaceIndex).ToUpper();
            string content = rawMessage.Remove(0, firstSpaceIndex + 1);

            if (rawMessage.StartsWith('/')) {
                return new IrcCommandEventArgs(command.Remove(0, 1), content);
            } else {
                return new IrcCommandEventArgs(Commands.PRIVMSG, "#testgrounds " + rawMessage);
            }
        }

        #endregion
    }
}
