using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Net.Event;

namespace Convex.IRC.Net {
    public interface IConnection {
        IAddress Address { get; }
        bool IsConnected { get; }
        bool IsInitialised { get; }
        bool Executing { get; set; }

        event AsyncEventHandler<ConnectedEventArgs> Connected;
        event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        event AsyncEventHandler<StreamFlushedEventArgs> Flushed;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;

        void Dispose();
        Task Initialise(IAddress address);
        Task SendDataAsync(object sender, IrcCommandEventArgs args);
        Task<string> ListenAsync();
    }
}