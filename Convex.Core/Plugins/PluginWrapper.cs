#region

using System;
using System.Threading.Tasks;
using Convex.Core.Events;
using Convex.Core.Net;
using Convex.Core.Plugins.Compositions;
using Serilog;
using SharpConfig;

#endregion

namespace Convex.Core.Plugins
{
    public class PluginHostWrapper
    {
        public PluginHostWrapper(Configuration configuration, string pluginMask)
        {
            Host = new PluginHost(configuration, pluginMask);
        }

        #region METHODS

        private async Task Callback(object sender, PluginActionEventArgs args)
        {
            if (Host.ShuttingDown)
            {
                return;
            }

            switch (args.ActionType)
            {
                case PluginActionType.Terminate:
                    await OnTerminated(sender, new OperationTerminatedEventArgs(sender, "Terminated."));
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(args.Result is IAsyncComposition<ServerMessagedEventArgs>))
                    {
                        break;
                    }

                    Host.RegisterComposition((IAsyncComposition<ServerMessagedEventArgs>)args.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(args.Result is CommandEventArgs))
                    {
                        break;
                    }

                    await OnCommandReceived(sender, (CommandEventArgs)args.Result);
                    break;
                case PluginActionType.Log:
                    if (!(args.Result is string))
                    {
                        break;
                    }

                    Log.Information((string)args.Result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region INIT

        public async Task Load()
        {
            if (Host == null)
            {
                return;
            }

            Host.PluginCallback += Callback;
            await Host.LoadPlugins();
            Host.StartPlugins();

            IsInitialized = true;
        }

        #endregion

        #region MEMBERS

        public PluginHost Host { get; }
        public bool IsInitialized { get; private set; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<CommandEventArgs> CommandReceived;
        public event AsyncEventHandler<OperationTerminatedEventArgs> Terminated;

        private async Task OnCommandReceived(object sender, CommandEventArgs args)
        {
            if (CommandReceived == null)
            {
                return;
            }

            await CommandReceived.Invoke(sender, args);
        }

        private async Task OnTerminated(object sender, OperationTerminatedEventArgs args)
        {
            if (Terminated == null)
            {
                return;
            }

            await Terminated.Invoke(sender, args);
        }

        #endregion
    }
}