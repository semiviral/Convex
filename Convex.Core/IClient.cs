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
    public interface IClient : IDisposable
    {
        string UniqueId { get; }
        Version AssemblyVersion { get; }
        bool Initialized { get; }
        IInitializedClient InitializedClient { get; }

        event AsyncEventHandler<DatabaseQueriedEventArgs> DatabaseQueried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        event AsyncEventHandler<ServerMessagedEventArgs> ServerMessaged;

        Task<IInitializedClient> Initialize();
        void RegisterMethod(IAsyncComposition<ServerMessagedEventArgs> methodRegistrar);
    }
}
