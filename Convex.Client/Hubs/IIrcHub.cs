using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convex.Client.Hubs {
    public interface IIrcHub {
        Task ReceiveBroadcastMessage(string rawMessage);
        Task ReceiveBroadcastMessageBatch(IEnumerable<string> rawMessages);
        Task ReceiveBroadcastMessageBatchPrepend(IEnumerable<string> rawMessages);
    }
}