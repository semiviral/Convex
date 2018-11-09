using System;

namespace Convex.Client.Model.Collections {
    public class MessagesIndex : IComparable<MessagesIndex> {
        public int Index { get; }
        public DateTime Timestamp { get; }
        public string ChannelName { get; }

        public MessagesIndex(int index, DateTime timestamp, string channelName) {
            Index = index;
            Timestamp = timestamp;
            ChannelName = channelName;
        }

        public int CompareTo(MessagesIndex mIndex) {
            if (mIndex == null) {
                return 0;
            }

            if (Index > mIndex.Index) {
                return -1;
            } else if (Index < mIndex.Index) {
                return 1;
            }

            return 0;
        }
    }
}
