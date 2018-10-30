using System.Threading.Tasks;
using Convex.Event;

namespace Convex.IRC.Net {
    public interface IServer {
        IConnection Connection { get; }
        bool Executing { get; }
        bool Identified { get; set; }
        bool Initialised { get; }

        event AsyncEventHandler<ServerMessagedEventArgs> ServerMessaged;

        void Dispose();
        Task Initialise(IAddress address);
        Task SendConnectionInfo(string nickname, string realname);

        Task ListenAsync(IIrcClient caller);
    }
}