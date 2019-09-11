#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convex.Core.Net;
using Convex.Event;
using Convex.Plugin.Composition;

#endregion

namespace Convex.Core
{
    public interface IIrcClient
    {
        string Address { get; }
        bool Initialising { get; }
        bool IsInitialised { get; }
        Dictionary<string, CompositionDescription> LoadedDescriptions { get; }
        int Port { get; }
        Server Server { get; }
        Guid UniqueId { get; }
        Version Version { get; }

        event AsyncEventHandler<ErrorEventArgs> Error;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        Task BeginListenAsync();
        bool CommandExists(string command);
        void Dispose();
        CompositionDescription GetDescription(string command);
        Task<bool> Initialise(IAddress address);
        void RegisterMethod(IAsyncCompsition<ServerMessagedEventArgs> methodRegistrar);
    }
}
