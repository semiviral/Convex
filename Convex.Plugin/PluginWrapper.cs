#region usings

using System;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    public class PluginWrapper<T> where T : EventArgs {
        #region MEMBERS

        public PluginHost<T> Host { get; }
        public bool Initialised { get; private set; }

        #endregion

        public PluginWrapper(string pluginsDirectory, Func<T, Task> onInvokedMethod) {
            Host = new PluginHost<T>(pluginsDirectory, onInvokedMethod);
        }


        #region METHODS

        private async Task Callback(object sender, PluginActionEventArgs e) {
            if (Host.ShuttingDown)
                return;

            switch (e.ActionType) {
                case PluginActionType.SignalTerminate:
                    await OnTerminated(sender, new OperationTerminatedEventArgs(sender, "Terminate signaled."));
                    break;
                case PluginActionType.RegisterMethod:
                    if (!(e.Result is IAsyncRegistrar<T>))
                        break;

                    Host.RegisterMethod((IAsyncRegistrar<T>)e.Result);
                    break;
                case PluginActionType.SendMessage:
                    if (!(e.Result is IrcCommandRecievedEventArgs))
                        break;

                    await OnCommandReceived(sender, (IrcCommandRecievedEventArgs)e.Result);
                    break;
                case PluginActionType.Log:
                    if (!(e.Result is string))
                        break;

                    await OnLog(sender, new InformationLoggedEventArgs((string)e.Result));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion


        #region INIT

        public async Task Initialise() {
            if (Host == null)
                return;

            Host.Logged += OnLog;
            Host.PluginCallback += Callback;
            await Host.LoadPlugins();
            Host.StartPlugins();

            Initialised = true;
        }

        #endregion


        #region EVENTS

        public event AsyncEventHandler<InformationLoggedEventArgs> Logged;
        public event AsyncEventHandler<IrcCommandRecievedEventArgs> CommandRecieved;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;

        private async Task OnLog(object sender, InformationLoggedEventArgs args) {
            if (Logged == null)
                return;

            await Logged.Invoke(sender, args);
        }

        private async Task OnCommandReceived(object sender, IrcCommandRecievedEventArgs args) {
            if (CommandRecieved == null)
                return;

            await CommandRecieved.Invoke(sender, args);
        }

        private async Task OnTerminated(object sender, OperationTerminatedEventArgs args) {
            if (TerminateSignaled == null)
                return;

            await TerminateSignaled.Invoke(sender, args);
        }

        #endregion
    }
}
