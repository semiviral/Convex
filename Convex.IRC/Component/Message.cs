using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convex.IRC.Net;

namespace Convex.Client.Component {
    public class Message : IMessage {
        public Message(string rawData, ref Func<ServerMessage, string> formatter) {
            RawMessage = rawData;
            Formatted = formatter.Invoke(this);

            if (rawData.StartsWith("ERROR")) {
                Command = Commands.ERROR;
                Args = rawData.Substring(rawData.IndexOf(' ') + 1);
                return;
            }

            Parse();
        }
        public string Formatted => throw new NotImplementedException();

        public string Nickname => throw new NotImplementedException();

        public string Origin => throw new NotImplementedException();

        public string RawMessage { get; }

        public DateTime Timestamp => throw new NotImplementedException();
    }
}
