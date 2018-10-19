using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.IRC.Component.Event;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHub : Hub<IIrcHub> {
        private readonly IIrcHostedService _ircService;
        private readonly CancellationToken _isCanceled;

        public IrcHub(IIrcHostedService ircHostedService) {
            _isCanceled = new CancellationToken(false);
            _ircService = ircHostedService;
        }


        public async Task BroadcastMessage(string message) {
            await Clients.All.ReceiveBroadcastMessage(message);
        }

        public async Task BroadcastMessagesToNewClient(string connectionId) {
            await Clients.Client(connectionId).ReceiveBroadcastMessages(_ircService.Messages.Select(message => message.ToString()).ToArray());
        }

        public override async Task OnConnectedAsync() {
            if (!_ircService.Client.IsInitialised && !_ircService.Client.Initialising) {
                _ircService.Client.Server.ServerMessaged += OnIrcServiceServerMessaged;
                await _ircService.StartAsync(_isCanceled);
            } else {
                while (!_ircService.Client.IsInitialised) Thread.Sleep(1000);

                await BroadcastMessagesToNewClient(Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        #region EVENT

        private async Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            await BroadcastMessage(args.Message.ToString());
        }

        #endregion
    }
}