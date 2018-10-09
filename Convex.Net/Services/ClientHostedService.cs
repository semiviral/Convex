using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.IRC.Dependency;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public class ClientHostedService : IHostedService, IClientHostedService
    {
        public ClientHostedService(string address, int port, Configuration configuration = null) {
            Address = address;
            Port = port;

            Messages = new List<ServerMessage>();
            Client = new IRC.Client();
        }

        #region EVENT

        private Task OnClientChannelMessaged(object sender, ServerMessagedEventArgs args) {
            Messages.Add(args.Message);

            Debug.WriteLine(args.Message.RawMessage);

            return Task.CompletedTask;
        }

        #endregion

        #region METHODS

        public IEnumerable<ServerMessage> GetAllMessages() {
            return Messages;
        }

        #endregion

        #region INIT

        private void Initialise() {
            Client.Server.ChannelMessaged += OnClientChannelMessaged;
            Client.Logged += (sender, args) => {
                Debug.WriteLine(args.Information);
                return Task.CompletedTask;
            };
            Client.Initialise(Address, Port);
        }

        #endregion

        #region MEMBERS

        public IClient Client { get; }

        public string Address { get; }
        public int Port { get; }

        private List<ServerMessage> Messages { get; }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public async Task StartAsync(CancellationToken cancellationToken) {
            Initialise();
            await Client.BeginListenAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Client.Dispose();
            return Task.CompletedTask;
        }

        #endregion
    }
}