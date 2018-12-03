using System.Collections.Generic;
using Convex.Client.Component;
using Convex.Client.Component.Collections;
using Convex.IRC.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Convex.Client.Model {
    [BindProperties]
    public class IndexModel : PageModel {
        private IrcChat Chat { get; }

        public string MessageInput { get; set; }
        public SortedList<MessagesIndex, ServerMessage> Messages { get; }

        public IndexModel() {
            Chat = new IrcChat();
            Chat.BroadcastMessageReceieved += (source, kvp) => AddMessage(kvp.Key, kvp.Value);
            Chat.MessageInputReceieved += (source, input) => UpdateMessageInput(input);

            MessageInput = string.Empty;
            Messages = new SortedList<MessagesIndex, ServerMessage>();
        }

        private void AddMessage(MessagesIndex index, ServerMessage message) {
            Messages.Add(index, message);
        }

        private void UpdateMessageInput(string input) {
            MessageInput = input;
        }


    }
}
