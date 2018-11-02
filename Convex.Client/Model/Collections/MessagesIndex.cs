using System;

namespace Convex.Client.Model.Collections {
    public class MessagesIndex : IComparable {
        public int Index { get; }
        public DateTime Timestamp { get; }
        public string ChannelName { get; }

        public MessagesIndex(int index, DateTime timestamp, string channelName) {
            Index = index;
            Timestamp = timestamp;
            ChannelName = channelName;
        }

        public int CompareTo(object obj) {
            MessagesIndex objIndex = (MessagesIndex)obj;

            if (Index > objIndex.Index) {
                return -1;
            } else if (Index < objIndex.Index) {
                return 1;
            } else {
                return 0;
            }
        }
    }
}
