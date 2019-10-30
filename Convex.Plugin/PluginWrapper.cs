#region

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Serilog;
using SharpConfig;

#endregion

namespace Convex.Plugin
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
                case PluginActionType.SignalTerminate:
                    await OnTerminated(source, new OperationTerminatedEventArgs(source, "Terminate signaled."));
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(args.Result is IAsyncComposition<T>))
                    {
                        break;
                    }

                    Host.RegisterComposition((IAsyncComposition<T>)args.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(args.Result is IrcCommandEventArgs))
                    {
                        break;
                    }

                    await OnCommandReceived(source, (IrcCommandEventArgs)args.Result);
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

            Initialized = true;
        }

        #endregion

        #region MEMBERS

        public PluginHost<T> Host { get; }
        public bool Initialized { get; private set; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<IrcCommandEventArgs> CommandReceived;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        private async Task OnCommandReceived(object source, IrcCommandEventArgs args)
        {
            if (CommandReceived == null)
            {
                return;
            }

            await CommandReceived.Invoke(source, args);
        }

        private async Task OnTerminated(object source, OperationTerminatedEventArgs args)
        {
            if (TerminateSignaled == null)
            {
                return;
            }

            await TerminateSignaled.Invoke(source, args);
        }

        #endregion
    }
}
