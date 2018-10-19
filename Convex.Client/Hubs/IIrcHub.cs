using System.Threading.Tasks;
using Convex.IRC.Component;

namespace Convex.Client.Hubs
{
    public interface IIrcHub {
        Task ReceiveBroadcastMessage(string message);
        Task ReceiveBroadcastMessages(string[] messages);
    }
}
