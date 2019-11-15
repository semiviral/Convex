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
    public class PluginHostWrapper<T> where T : EventArgs
    {
        public PluginHostWrapper(Configuration configuration, Func<InvokedAsyncEventArgs<T>, Task> invokeAsyncMethod,
            string pluginMask) => Host = new PluginHost<T>(configuration, invokeAsyncMethod, pluginMask);

        #region METHODS

        private async Task Callback(object source, PluginActionEventArgs args)
        {
            if (Host.ShuttingDown)
            {
                return;
            }

            switch (args.ActionType)
            {
                case PluginActionType.Terminate:
                    await OnTerminated(source, new OperationTerminatedEventArgs(source, "Terminated."));
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(args.Result is IAsyncComposition<T>))
                    {
                        break;
                    }

                    Host.RegisterComposition((IAsyncComposition<T>)args.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(args.Result is CommandEventArgs))
                    {
                        break;
                    }

                    await OnCommandReceived(source, (CommandEventArgs)args.Result);
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

        public async Task Initialize()
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

        public PluginHost<T> Host { get; }
        public bool IsInitialized { get; private set; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<CommandEventArgs> CommandReceived;
        public event AsyncEventHandler<OperationTerminatedEventArgs> Terminated;

        private async Task OnCommandReceived(object source, CommandEventArgs args)
        {
            if (CommandReceived == null)
            {
                return;
            }

            await CommandReceived.Invoke(source, args);
        }

        private async Task OnTerminated(object source, OperationTerminatedEventArgs args)
        {
            if (Terminated == null)
            {
                return;
            }

            await Terminated.Invoke(source, args);
        }

        #endregion
    }
}
