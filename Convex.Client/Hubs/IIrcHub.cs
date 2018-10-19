using System.Threading.Tasks;

namespace Convex.Client.Hubs {
    public interface IIrcHub {
        Task ReceiveBroadcastMessage(string message);
    }
}