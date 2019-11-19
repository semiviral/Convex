using System;
using System.Threading.Tasks;
using Convex.Core.Events;

namespace Convex.Core.Net
{
    public interface IConnection : IDisposable
    {
        bool Connected { get; }
        IEstablishedConnection EstablishedConnection { get; }

        event AsyncEventHandler<ConnectedEventArgs> Established;
        event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        event AsyncEventHandler<StreamDataEventArgs> DataWritten;
        event AsyncEventHandler<StreamDataEventArgs> DataReceived;

        Task<IEstablishedConnection> Connect(IAddress address);
    }
}