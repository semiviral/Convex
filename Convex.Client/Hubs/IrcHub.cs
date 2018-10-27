using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Services;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHub : Hub<IIrcHub> {
        public IrcHub(IIrcHubProxyService ircHubProxyService) {
            _isCanceled = new CancellationToken(false);
            _ircHubProxyService = ircHubProxyService;
        }

        #region OVERRIDES

        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();

            await _ircHubProxyService.BroadcastMessageBatch(Context.ConnectionId, false, 0, 200);
        }

        #endregion

        #region MEMBERS

        private readonly IIrcHubProxyService _ircHubProxyService;
        private readonly CancellationToken _isCanceled;

        #endregion

        #region RELAY METHODS

        public async Task SendMessage(string rawMessage) {
            await _ircHubProxyService.SendMessage(rawMessage);
        }

        #endregion
    }
}