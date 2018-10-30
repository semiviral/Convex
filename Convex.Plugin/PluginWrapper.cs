#region USINGS

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;
using Convex.Plugin.Registrar;
using Convex.Util;
using Serilog.Events;


#endregion

namespace Convex.Plugin {
    public class PluginWrapper<T> where T : EventArgs {
        public PluginWrapper(string pluginsDirectory, Func<T, Task> onInvokedMethod) {
            Host = new PluginHost<T>(pluginsDirectory, onInvokedMethod);
        }

        #region METHODS

        private async Task Callback(object sender, PluginActionEventArgs args) {
            if (Host.ShuttingDown) {
                return;
            }

            switch (args.ActionType) {
                case PluginActionType.SignalTerminate:
                    await OnTerminated(sender, new OperationTerminatedEventArgs(sender, "Terminate signaled."));
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(args.Result is IAsyncRegistrar<T>)) {
                        break;
                    }

                    Host.RegisterMethod((IAsyncRegistrar<T>)args.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(args.Result is IrcCommandEventArgs)) {
                        break;
                    }

                    await OnCommandReceived(sender, (IrcCommandEventArgs)args.Result);
                    break;
                case PluginActionType.Log:
                    if (!(args.Result is string)) {
                        break;
                    }

                    StaticLog.Log(new LogEventArgs(LogEventLevel.Information, (string)args.Result));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region INIT

        public async Task Initialise() {
            if (Host == null) {
                return;
            }

            Host.PluginCallback += Callback;
            await Host.LoadPlugins();
            Host.StartPlugins();

            Initialised = true;
        }

        #endregion

        #region MEMBERS

        public PluginHost<T> Host { get; }
        public bool Initialised { get; private set; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<IrcCommandEventArgs> CommandReceived;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        private async Task OnCommandReceived(object sender, IrcCommandEventArgs args) {
            if (CommandReceived == null) {
                return;
            }

            await CommandReceived.Invoke(sender, args);
        }

        private async Task OnTerminated(object sender, OperationTerminatedEventArgs args) {
            if (TerminateSignaled == null) {
                return;
            }

            await TerminateSignaled.Invoke(sender, args);
        }

        #endregion
    }
}