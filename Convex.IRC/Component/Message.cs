using System;
using Convex.IRC.Net;

namespace Convex.Client.Component {
    public class Message : IMessage {
        public Message(string rawData) {
            Timestamp = DateTime.UtcNow;
            RawMessage = rawData;
            Origin = Nickname = "System";
        }


        public Message(string rawData, ref Func<IMessage, string> formatter) : this(rawData) {
            Formatted = formatter.Invoke(this);
        }

        #region MEMBERS

        public DateTime Timestamp { get; }
        public string RawMessage { get; }
        public string Formatted { get; }
        public string Origin { get; }
        public string Nickname { get; }

        #endregion
    }
}
