using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.IRC.Component.Event;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHub : Hub<IIrcHub> {
        private readonly IIrcHostedService _ircService;
        private CancellationToken _isCanceled;

        public IrcHub(IIrcHostedService ircHostedService) {
            _isCanceled = new CancellationToken(false);

            _ircService = ircHostedService;
            _ircService.Client.Server.ServerMessaged += OnIrcServiceServerMessaged;
            _ircService.StartAsync(_isCanceled);
        }

        public async Task BroadcastMessage(string message) {
            await Clients.All.ReceiveBroadcastMessage(message);
        }

        public async Task BroadcastMessagesToNewClient(string connectionId) {
            await Clients.Client(connectionId).ReceiveBroadcastMessages(_ircService.Messages.Select(message => message.ToString()).ToArray());
        }

        public override async Task OnConnectedAsync() {
            await BroadcastMessagesToNewClient(Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        #region EVENT

        private async Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            await BroadcastMessage(args.Message.ToString());
        }

        #endregion
    }
}