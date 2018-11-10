using System.Collections.Generic;
using BlazorSignalR;
using Convex.Client.Component.Collections;
using Convex.IRC.Net;
using Microsoft.AspNetCore.SignalR.Client;

namespace Convex.Client.Model {
    public class IrcChat {
        private HubConnection _connection { get; } = new HubConnectionBuilder().WithUrlBlazor("/IrcHub").Build();
        public SortedList<MessagesIndex, ServerMessage> Messages { get; } = new SortedList<MessagesIndex, ServerMessage>();
        public string Message { get; set; } = string.Empty;

        public IrcChat() {

        }
    }
}
