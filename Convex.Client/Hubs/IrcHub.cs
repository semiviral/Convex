using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Proxy;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs {
    public class IrcHub : Hub<IIrcHub> {
        public IrcHub(IIrcHubProxy ircHubProxy) {
            _isCanceled = new CancellationToken(false);
            _ircHubProxy = ircHubProxy;
        }

        #region OVERRIDES

        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();

            await _ircHubProxy.BroadcastMessageBatch(Context.ConnectionId, false, 0, 200);
        }

        #endregion

        #region MEMBERS

        private readonly IIrcHubProxy _ircHubProxy;
        private readonly CancellationToken _isCanceled;

        #endregion

        #region RELAY METHODS

        public async Task SendMessage(string rawMessage) {
            await _ircHubProxy.SendMessage(rawMessage);
        }

        /// <summary>
        /// Updated the text of the MessageInput box
        /// </summary>
        /// <param name="increment">whether the operation is incremental or decremental</param>
        /// <param name="updatedInput"></param>
        /// <returns></returns>
        public async Task UpdateMessageInput(bool increment) {
            await _ircHubProxy.UpdateMessageInput(Context.ConnectionId, increment);
        }

        #endregion
    }
}