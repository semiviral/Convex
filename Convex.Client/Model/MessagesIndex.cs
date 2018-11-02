using System;

namespace Convex.Client.Model {
    public class MessagesIndex {
        private int Index { get; }
        private DateTime Timestamp { get; }
        private string ChannelName { get; }

        public MessagesIndex(int index, DateTime timestamp, string channelName) {
            Index = index;
            Timestamp = timestamp;
            ChannelName = channelName;
        }
    }
}
