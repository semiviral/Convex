using System;

namespace Convex.Client.Component.Collections {
    public class MessagesIndex : IComparable<MessagesIndex> {
        public int Index { get; }
        public DateTime Timestamp { get; }
        public string ChannelName { get; }
        public bool Inverted { get; set; }

        public MessagesIndex(int index, DateTime timestamp, string channelName, bool inverted) {
            Index = index;
            Timestamp = timestamp;
            ChannelName = channelName;
            Inverted = inverted;
        }

        public int CompareTo(MessagesIndex other) {
            if (other == null) {
                return 0;
            }

            if (Inverted) {
                if (Index > other.Index) {
                    return 1;
                } else if (Index < other.Index) {
                    return -1;
                }
            } else {
                if (Index > other.Index) {
                    return -1;
                } else if (Index < other.Index) {
                    return 1;
                }
            }

            return 0;
        }
    }
}
