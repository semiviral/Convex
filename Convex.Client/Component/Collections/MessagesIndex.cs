using System;

namespace Convex.Client.Component.Collections {
    public class MessagesIndex : IComparable<MessagesIndex> {
        public int Index { get; }
        public DateTime Timestamp { get; }
        public string ChannelName { get; }

        public MessagesIndex(int index, DateTime timestamp, string channelName) {
            Index = index;
            Timestamp = timestamp;
            ChannelName = channelName;
        }

        public int CompareTo(MessagesIndex other) {
            if (other == null) {
                return 0;
            }

            if (Index > other.Index) {
                return -1;
            } else if (Index < other.Index) {
                return 1;
            }

            return 0;
        }
    }
}
