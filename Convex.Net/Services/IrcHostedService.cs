using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.IRC.Component.Reference;
using Convex.IRC.Dependency;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace Convex.Client.Services {
    public class IrcHostedService : IHostedService, IDisposable
    {
        public IrcHostedService() {
            Client = new IRC.Client();
            Messages = new List<ServerMessage>();
            Connection = new HubConnectionBuilder().WithUrl("/IrcHub").Build();

            Connection.Closed += async error => {
                await Task.Delay(new Random().Next(0, 5) * 1000));
                await Connection.StartAsync();
            };

            Address = "irc.foonetic.net";
            Port = 6667;
        }

        #region EVENT

        private Task OnIrcServiceServerMessaged(object sender, ServerMessagedEventArgs args) {
            Messages.Add(args.Message);

            Debug.WriteLine(args.Message.RawMessage);

            return Task.CompletedTask;
        }

        #endregion

        #region METHODS

        private async Task DoWork() {
            await Client.BeginListenAsync();
        }

        #endregion

        #region INIT

        private void Initialise() {
            Client.Server.ServerMessaged += OnIrcServiceServerMessaged;
            Client.Logged += (sender, args) => {
                Debug.WriteLine(args.Information);
                return Task.CompletedTask;
            };
            Client.Initialise(Address, Port);
        }

        private async Task InitialiseHub() {
            Connection.On<string>("ReceiveMessage", message => { Client.Server.Connection.SendDataAsync(this, new IrcCommandReceivedEventArgs(Commands.PRIVMSG, message)); });

            try {
                await Connection.StartAsync();

            }
        }

        #endregion

        #region MEMBERS
            
        public HubConnection Connection { get; }
        public IClient Client { get; }

        public string Address { get; }
        public int Port { get; }

        private List<ServerMessage> Messages { get; }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public async Task StartAsync(CancellationToken cancellationToken) {
            Initialise();

            co

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