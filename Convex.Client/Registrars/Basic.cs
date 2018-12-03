using System;
using System.Linq;
using System.Threading.Tasks;
using Convex.Client.Model;
using Convex.IRC;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;

namespace Convex.Client.Registrars {
    public class Basic {
        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, Eve is the only one in the channel
            if (e.Message.SplitArgs.Count < 4) {
                return Task.CompletedTask;
            }

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Server.Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null || currentChannel.Inhabitants.Contains(s)) {
                    continue;
                }

                Server?.Channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }

            return Task.CompletedTask;
        }
    }
}
