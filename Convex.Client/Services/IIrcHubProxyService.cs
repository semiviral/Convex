using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Convex.Client.Services {
    public interface IIrcHubProxyService : IHostedService {
        Task SendMessage(string rawMessage);
        Task BroadcastMessageBatch(string connectionId, int startIndex, int endIndex, bool isPrepend);
    }
}