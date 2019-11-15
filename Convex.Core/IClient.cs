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

        Task<IInitializedClient> Initialize();
        void RegisterMethod(IAsyncComposition<ServerMessagedEventArgs> methodRegistrar);

        event AsyncEventHandler<ClassInitializedEventArgs> InitializationCompleted;
        event AsyncEventHandler<DatabaseQueriedEventArgs> DatabaseQueried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
    }
}
