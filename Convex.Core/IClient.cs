#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convex.Core.Events;
using Convex.Core.Net;
using Convex.Core.Plugins.Compositions;

#endregion

namespace Convex.Core
{
    public interface IClient
    {
        string Address { get; }
        bool Initializing { get; }
        bool IsInitialized { get; }
        IReadOnlyDictionary<string, CompositionDescription> PluginCommands { get; }
        int Port { get; }
        Server Server { get; }
        Guid UniqueId { get; }
        Version Version { get; }

        event AsyncEventHandler<ErrorEventArgs> Error;
        event AsyncEventHandler<ClassInitializedEventArgs> Initialized;
        event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        Task BeginListenAsync();
        bool CommandExists(string command);
        void Dispose();
        CompositionDescription GetDescription(string command);
        Task<bool> Initialize(IAddress address);
        void RegisterMethod(IAsyncComposition<ServerMessagedEventArgs> methodRegistrar);
    }
}
