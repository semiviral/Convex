#region

using System;

#endregion

namespace Convex.Core.Component
{
    public class Message : IMessage
    {
        public Message(string rawData)
        {
            Timestamp = DateTime.UtcNow;
            RawMessage = Formatted = rawData;
            Origin = Nickname = "System";
        }

        public Message(string rawData, string origin = "System", string nickname = "System") : this(rawData)
        {
            Origin = origin;
            Nickname = nickname;
        }

        #region MEMBERS

        public DateTime Timestamp { get; protected set; }
        public string RawMessage { get; protected set; }
        public string Formatted { get; protected set; }
        public string Origin { get; set; }
        public string Nickname { get; set; }

        #endregion
    }
}