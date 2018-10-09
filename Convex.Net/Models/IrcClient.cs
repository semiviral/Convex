using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.IRC;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.IRC.Dependency;

namespace Convex.Clients.Models {
    public class IrcClient : IIrcClient {
        public IrcClient(string address, int port, Configuration configuration = null) {
            Address = address;
            Port = port;

            Messages = new List<ServerMessage>();

            Client = new Client();
            Client.Server.ChannelMessaged += OnClientChannelMessaged;
            ThreadPool.QueueUserWorkItem(async delegate { await RunIrcService(); });
            Client.Initialise(Address, Port, configuration);
        }

        #region EVENT

        private Task OnClientChannelMessaged(object sender, ServerMessagedEventArgs args) {
            Messages.Add(args.Message);

            Debug.WriteLine(args.Message.RawMessage);

            return Task.CompletedTask;
        }

        #endregion
        
        #region MEMBERS

        public IClient Client { get; }

        public string Address { get; }
        public int Port { get; }

        private List<ServerMessage> Messages { get; }

        #endregion

        #region METHODS

        private async Task RunIrcService() {
            await Client.Initialise(Address, Port);
            await Client.BeginListenAsync();
        }

        public IEnumerable<ServerMessage> GetAllMessages() {
            return Messages.AsReadOnly();
        }

        public IEnumerable<ServerMessage> GetMessagesByDateTimeOrDefault(DateTime referenceTime, DateTimeOrdinal dateTimeOrdinal) {
            IEnumerable<ServerMessage> temporaryList = null;

            switch (dateTimeOrdinal) {
                case DateTimeOrdinal.Before:
                    temporaryList = Messages.Where(message => message.Timestamp < referenceTime);
                    break;
                case DateTimeOrdinal.After:
                    temporaryList = Messages.Where(message => message.Timestamp > referenceTime);
                    break;
            }

            return temporaryList;
        }

        #endregion
    }

    public enum DateTimeOrdinal {
        Before,
        After
    }
}