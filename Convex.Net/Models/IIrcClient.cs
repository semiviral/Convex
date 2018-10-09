using System.Collections.Generic;
using Convex.IRC.Component;
using Convex.IRC.Dependency;

namespace Convex.Clients.Models {
    public interface IIrcClient {
        string Address { get; }
        IClient Client { get; }
        int Port { get; }
        
        IEnumerable<ServerMessage> GetAllMessages();
    }
}