using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Component;
using Convex.IRC.Dependency;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public class IrcService : IHostedService, IIrcService {
        public IrcService() {
            _firstMessagesEntry = false;
            Client = new IRC.Client();
            Messages = new SortedList<int, ServerMessage>();

            Client.Logged += (sender, args) => {
                Debug.WriteLine(args.Information);

                return Task.CompletedTask;
            };

            Client.Server.ServerMessaged += (sender, args) => {
                if (_firstMessagesEntry) {
                    Messages.Add(Messages.Keys.Max() + 1, args.Message);
                } else {
                    Messages.Add(0, args.Message);

                    _firstMessagesEntry = true;
                }

                Debug.WriteLine(args.Message.RawMessage);

                return Task.CompletedTask;
            };

            Address = "irc.foonetic.net";
            Port = 6667;
        }

        #region METHODS

        private async Task DoWork() {
            await Client.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        private bool _firstMessagesEntry;

        public IClient Client { get; }

        public string Address { get; }
        public int Port { get; }

        public SortedList<int, ServerMessage> Messages { get; }

        #endregion

        #region INIT

        private async Task Initialise() {
            await InitialiseClient();
        }

        private async Task InitialiseClient() {
            await Client.Initialise(Address, Port);
        }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public async Task StartAsync(CancellationToken cancellationToken) {
            if (Client.IsInitialised) {
                return;
            }

            await Initialise();
            await DoWork();
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Dispose();

            return Task.CompletedTask;
        }

        public void Dispose() {
            Client.Dispose();
        }

        #endregion
    }
}