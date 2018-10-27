using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC.Component;
using Convex.IRC.Dependency;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Convex.Client.Services {
    public class IrcService : IHostedService, IIrcService {
        public IrcService() {
            Client = new IRC.Client(Program.Config);
            Messages = new SortedList<Tuple<int, DateTime>, ServerMessage>();

            Client.Logged += (sender, args) => {
                Debug.WriteLine(args.Information);

                return Task.CompletedTask;
            };

            Client.Server.ServerMessaged += (sender, args) => {
                Messages.Add(new Tuple<int, DateTime>(GetMaxIndex(), args.Message.Timestamp), args.Message);

                Debug.WriteLine(args.Message.RawMessage);

                return Task.CompletedTask;
            };

            Address = "irc.foonetic.net";
            Port = 6667;
        }

        #region EVENT

        public void Log(string information) {

        }

        #endregion

        #region METHODS

        private async Task DoWork() {
            await Client.BeginListenAsync();
        }

        public int GetMaxIndex() {
            return Messages.Keys.Select(tup => tup.Item1).Max();
        }

        #endregion

        #region MEMBERS

        public IClient Client { get; }

        public string Address { get; }
        public int Port { get; }

        public SortedList<Tuple<int, DateTime>, ServerMessage> Messages { get; }

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