using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Convex.IRC;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;

namespace Convex.Client.Model {
    public class IrcClientWrapper {
        public IrcClientWrapper(Configuration config = null) {
            Channels = new ObservableCollection<Channel>();

            Client = new IrcClient(config);

        }

        public ObservableCollection<Channel> Channels { get; }
        public IrcClient Client { get; }

        #region REGISTRARS

        private Task NamesReply(ServerMessagedEventArgs e) {
            string channelName = e.Message.SplitArgs[1];

            // * SplitArgs [2] is always your nickname

            // in this case, you is the only one in the channel
            if (e.Message.SplitArgs.Count < 4) {
                return Task.CompletedTask;
            }

            foreach (string s in e.Message.SplitArgs[3].Split(' ')) {
                Channel currentChannel = Channels.SingleOrDefault(channel => channel.Name.Equals(channelName));

                if (currentChannel == null || currentChannel.Inhabitants.Contains(s)) {
                    continue;
                }

                Channels.Single(channel => channel.Name.Equals(channelName)).Inhabitants.Add(s);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
