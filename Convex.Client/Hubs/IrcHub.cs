using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.Event;
using Convex.IRC.Component.Event;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHub : Hub<IIrcHub> {
        public IrcHub(IIrcService ircService) {
            _isCanceled = new CancellationToken(false);
            _ircService = ircService;
            _ircService.Client.Server.ServerMessaged += OnIrcServiceServerMessaged;
        }

        #region OVERRIDES

        public override async Task OnConnectedAsync() {
            while (!_ircService.Client.IsInitialised) {
                await Task.Delay(200, _isCanceled);
            }

            await BroadcastMessageBatch(Context.ConnectionId, 0, 30, false);

            await base.OnConnectedAsync();
        }

        #endregion

        #region EVENT

        private async Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            await BroadcastMessage(args.Message.RawMessage);
        }

        #endregion

        #region MEMBERS

        private readonly IIrcService _ircService;
        private readonly CancellationToken _isCanceled;

        #endregion

        #region RELAY METHODS

        public async Task BroadcastMessage(string message) {
            Debug.WriteLine(message);
            await Clients.All.ReceiveBroadcastMessage(message);
        }

        /// <summary>
        ///     Broadcasts a batch of messages.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, int startIndex, int endIndex, bool isPrepended) {
            while (_ircService.Messages[endIndex] == null && endIndex > 0) {
                endIndex -= 1;
            }

            if (Clients.Client(connectionId) == null || startIndex >= endIndex || startIndex < 0 || endIndex < 0) {
                return;
            }



            IEnumerable<string> messageList = _ircService.Messages.Skip(startIndex).Take(endIndex - startIndex + 1).Select(message => message.Value.RawMessage);

            if (isPrepended) {
                await Clients.Client(connectionId).ReceiveBroadcastMessageBatch(messageList);
            } else {
                await Clients.Client(connectionId).ReceiveBroadcastMessageBatch(messageList);
            }
        }

        public async Task SendMessage(string rawMessage) {
            await _ircService.Client.Server.Connection.SendDataAsync(this, new IrcCommandEventArgs(string.Empty, rawMessage));
        }

        #endregion
    }
}