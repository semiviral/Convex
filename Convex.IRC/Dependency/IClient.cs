using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Component;
using Convex.IRC.Component.Event;
using Convex.Plugin.Registrar;

namespace Convex.IRC.Dependency {
    public interface IClient {
        string Address { get; }
        int Port { get; }
        List<string> IgnoreList { get; }
        bool IsInitialised { get; }
        Dictionary<string, Tuple<string, string>> LoadedCommands { get; }
        Server Server { get; }
        Version Version { get; }
        Configuration GetClientConfiguration();

        event AsyncEventHandler<ErrorEventArgs> Error;
        event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        event AsyncEventHandler<InformationLoggedEventArgs> Logged;
        event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        Task BeginListenAsync();
        void Dispose();
        string GetApiKey(string type);
        bool CommandExists(string command);
        Tuple<string, string> GetCommand(string command);
        Task<bool> Initialise(string address, int port, Configuration configuration = null);
        void RegisterMethod(IAsyncRegistrar<ServerMessagedEventArgs> methodRegistrar);
    }
}