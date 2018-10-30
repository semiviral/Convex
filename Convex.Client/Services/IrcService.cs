using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Model;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public class IrcService : IHostedService, IIrcService {
        public IrcService() {
            IrcClientWrapper = new IrcClientWrapper(Program.Config);

            Address = "irc.foonetic.net";
            Port = 6667;
        }

        #region METHODS

        private async Task DoWork() {
            await IrcClientWrapper.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        public IIrcClientWrapper IrcClientWrapper { get; }
        public string Address { get; }
        public int Port { get; }

        #endregion

        #region INIT

        public async Task Initialise() {
            await InitialiseClient();
            await DoWork();
        }

        private async Task InitialiseClient() {
            await IrcClientWrapper.Initialise(Address, Port);
        }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public Task StartAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Dispose();

            return Task.CompletedTask;
        }

        public void Dispose() {
            IrcClientWrapper.Dispose();
        }

        #endregion
    }
}