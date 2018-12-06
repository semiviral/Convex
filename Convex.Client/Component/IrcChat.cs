using System;
using System.Collections.Generic;
using BlazorSignalR;
using Convex.Client.Component.Collections;
using Convex.Core.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;

namespace Convex.Client.Component {
    public class IrcChat : PageModel {
        private HubConnection _connection { get; }

        public event EventHandler<KeyValuePair<MessagesIndex, ServerMessage>> BroadcastMessageReceieved;
        public event EventHandler<string> MessageInputReceieved;

        public IrcChat() {
            _connection = new HubConnectionBuilder().WithUrlBlazor("/IrcHub").Build();

            RegisterConnectionEvents();
        }

        private void RegisterConnectionEvents() {
            _connection.On<MessagesIndex, ServerMessage>("ReceieveBroadcastMessage", ProcessMessage);
            _connection.On<IDictionary<MessagesIndex, ServerMessage>>("ReceieveBroadcastMessageBatch", ProcessMessageBatch);
            _connection.On<string>("UpdateMessageInput", UpdateMessageInput);
        }

        private void ProcessMessage(MessagesIndex index, ServerMessage message) {
            BroadcastMessageReceieved(this, new KeyValuePair<MessagesIndex, ServerMessage>(index, message));
        }

        private void ProcessMessageBatch(IDictionary<MessagesIndex, ServerMessage> messageBatch) {
            foreach (KeyValuePair<MessagesIndex, ServerMessage> kvpMessage in messageBatch) {
                ProcessMessage(kvpMessage.Key, kvpMessage.Value);
            }
        }

        private void UpdateMessageInput(string input) {
            MessageInputReceieved.Invoke(this, input);
        }
    }
}
