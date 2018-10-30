using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.Plugin.Registrar;

namespace Convex.IRC {
    public interface IIrcClient {
        string Address { get; }
        List<string> IgnoreList { get; }
        bool IsInitialised { get; }
        bool Initialising { get; }
        Dictionary<string, Tuple<string, string>> LoadedCommands { get; }
        int Port { get; }
        Server Server { get; }
        Version Version { get; }
        Guid UniqueId { get; }

        event AsyncEventHandler<ErrorEventArgs> Error;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        event AsyncEventHandler<LogEventArgs> Logged;
        event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        Task BeginListenAsync();
        bool CommandExists(string command);
        void Dispose();
        string GetApiKey(string type);
        Configuration Config { get; }

        Tuple<string, string> GetCommand(string command);
        Task<bool> Initialise(string address, int port);
        void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar);
    }
}