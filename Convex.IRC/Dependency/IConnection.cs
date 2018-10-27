using System.Threading.Tasks;
using Convex.Event;

namespace Convex.IRC.Component.Net {
    public interface IConnection {
        string Address { get; }
        bool IsConnected { get; }
        bool IsInitialised { get; }
        bool Executing { get; set; }
        int Port { get; }

        event AsyncEventHandler<ConnectedEventArgs> Connected;
        event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        event AsyncEventHandler<StreamFlushedEventArgs> Flushed;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        event AsyncEventHandler<LogEventArgs> Logged;

        void Dispose();
        Task Initialise(string address, int port);
        Task SendDataAsync(object sender, IrcCommandEventArgs args);
        Task<string> ListenAsync();
    }
}