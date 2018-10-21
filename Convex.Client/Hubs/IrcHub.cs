using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.Event;
using Convex.IRC.Component;
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
            while (!_ircService.Client.IsInitialised) await Task.Delay(1000, _isCanceled);

            await BroadcastAllMessages(Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        #endregion

        #region EVENT

        private async Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            await BroadcastMessage(args.Message.ToString());
        }

        #endregion

        #region MEMBERS

        private readonly IIrcService _ircService;
        private readonly CancellationToken _isCanceled;

        #endregion

        #region RELAY METHODS

        public async Task BroadcastMessage(string message) {
            await Clients.All.ReceiveBroadcastMessage(message);
        }

        public async Task BroadcastAllMessages(string connectionId) {
            foreach (ServerMessage message in _ircService.Messages) await Clients.Client(connectionId).ReceiveBroadcastMessage(message.ToString());
        }

        public async Task SendMessage(string rawMessage) {
           await _ircService.Client.Server.Connection.SendDataAsync(this, new IrcCommandEventArgs(string.Empty, rawMessage));
        }

        #endregion
    }
}