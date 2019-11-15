#region

using System;
using System.Threading.Tasks;
using Convex.Core.Events;
using SharpConfig;

#endregion

namespace Convex.Core.Plugins
{
    /// <summary>
    ///     Interface for hooking a new plugin into Eve
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        ///     Often used to register methods
        /// </summary>
        Task Start(Configuration configuration);

        Task Stop();
        Task CallDie();

        event AsyncEventHandler<PluginActionEventArgs> Callback;

        #region MEMBERS

        string Name { get; }
        string Author { get; }
        Version Version { get; }
        string Id { get; }
        PluginStatus Status { get; }

        #endregion
    }

    public class PluginInstance
    {
        public PluginInstance(IPlugin instance, PluginStatus status)
        {
            Instance = instance;
            Status = status;
        }

        #region MEMBERS

        public readonly IPlugin Instance;
        public PluginStatus Status;

        #endregion
    }

    public enum PluginActionType
    {
        Log,
        RegisterMethod,
        SendMessage,
        Terminate
    }

    public enum PluginStatus
    {
        Stopped = 0,
        Running,
        Processing
    }
}
