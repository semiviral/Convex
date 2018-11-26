using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Net;
using Convex.Plugin.Composition;

namespace Convex.IRC {
    public interface IIrcClient {
        string Address { get; }
        IConfiguration Config { get; }
        List<string> IgnoreList { get; }
        bool Initialising { get; }
        bool IsInitialised { get; }
        Dictionary<string, CompositionDescription> LoadedCommands { get; }
        int Port { get; }
        IServer Server { get; }
        Guid UniqueId { get; }
        Version Version { get; }

        event AsyncEventHandler<ErrorEventArgs> Error;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        Task BeginListenAsync();
        bool CommandExists(string command);
        void Dispose();
        string GetApiKey(string type);
        CompositionDescription GetCommand(string command);
        Task<bool> Initialise(IAddress address);
        void RegisterMethod(IAsyncCompsition<ServerMessagedEventArgs> methodRegistrar);
    }
}