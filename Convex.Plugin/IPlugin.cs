#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;

#endregion

namespace Convex.Plugin {
    /// <summary>
    ///     Interface for hooking a new plugin into Eve
    /// </summary>
    public interface IPlugin {
        #region MEMBERS

        string Name { get; }
        string Author { get; }
        Version Version { get; }
        string Id { get; }
        PluginStatus Status { get; }

        #endregion

        /// <summary>
        ///     Often used to register methods
        /// </summary>
        Task Start();

        Task Stop();
        Task CallDie();

        event AsyncEventHandler<PluginActionEventArgs> Callback;
    }

    public class PluginInstance {
        #region MEMBERS

        public readonly IPlugin Instance;
        public PluginStatus Status;

        #endregion

        public PluginInstance(IPlugin instance, PluginStatus status) {
            Instance = instance;
            Status = status;
        }
    }

    public enum PluginActionType {
        Log,
        RegisterMethod,
        SendMessage,
        SignalTerminate
    }

    public enum PluginStatus {
        Stopped = 0,
        Running,
        Processing
    }
}
