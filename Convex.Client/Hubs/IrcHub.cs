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

        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();

            await RequestBroadcastMessageBatch(string.Empty, false, 0, 400);
        }

        #region MEMBERS

        private readonly IIrcHubProxy _ircHubProxy;
        private readonly CancellationToken _isCanceled;

        #endregion

        #region RELAY METHODS

        public async Task RequestBroadcastMessageBatch(string channelName, bool isPrepend, int startIndex, int endIndex) {
            await _ircHubProxy.BroadcastMessageBatch(Context.ConnectionId, isPrepend, channelName, startIndex, endIndex);
        }

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