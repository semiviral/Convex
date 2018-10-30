using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convex.IRC;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;

namespace Convex.Client.Model {
    public class IrcClientWrapper {
        public IrcClientWrapper(Configuration config = null) {
            Channels = new List<Channel>();

            Client = new IrcClient(config);

        }

        public List<Channel> Channels { get; }
        public IrcClient Client { get; }

        #region METHODS

        public Channel GetChannel(string channelName) {
            return Channels.Single(channel => channel.Name.Equals(channelName));
        }

        #endregion

        #region REGISTRARS

        private Task NamesReply(ServerMessagedEventArgs e) {
            char[] identifiers = { '~', '@', '+' };
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, you are the only one in the channel
            if (e.Message.SplitArgs.Count < 4) {
                return Task.CompletedTask;
            }

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = GetChannel(channelName);

                if (currentChannel == null || currentChannel == default(Channel)) {
                    continue;
                }



                GetChannel(channelName).Inhabitants.Add(default(User));
            }

            return Task.CompletedTask;
        }

        private Task Join(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin)?.Inhabitants.Add(default(User));
            return Task.CompletedTask;
        }
        private Task Part(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin)?.Inhabitants.RemoveAll(x => x.Equals(e.Message.Nickname));
            return Task.CompletedTask;
        }
        private Task ChannelTopic(ServerMessagedEventArgs e) {
            GetChannel(e.Message.SplitArgs[0]).Topic = e.Message.Args.Substring(e.Message.Args.IndexOf(' ') + 2);
            return Task.CompletedTask;
        }
        private Task NewTopic(ServerMessagedEventArgs e) {
            GetChannel(e.Message.Origin).Topic = e.Message.Args;
            return Task.CompletedTask;
        }

        #endregion
    }
}
