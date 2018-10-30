using System;

namespace Convex.IRC.Component {
    public class Message {
        public Message(int id, string sender, string contents, DateTime timestamp) {
            Id = id;
            Sender = sender;
            Contents = contents;
            Date = timestamp;
        }

        #region MEMBERS

        public int Id { get; }
        public string Sender { get; }
        public string Contents { get; }
        public DateTime Date { get; }

        #endregion
    }
}
