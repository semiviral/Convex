using System;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;
using Convex.Plugin.Registrar;
using Convex.Util;
using NodeNetwork;

namespace Convex.Plugin.Node_Network {
    public class LanguageReader : IPlugin {
        #region REGISTRARS

        private Task ProcessText(ServerMessagedEventArgs args) {
            Network.ProcessInput(args.Message.SplitArgs.ToArray());

            return Task.CompletedTask;
        }

        #endregion

        #region MEMBERS

        public string Name => "LanguageReader";
        public string Author => "Antonio DiNostri";
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;
        public string Id => Guid.NewGuid().ToString();
        public PluginStatus Status { get; private set; } = PluginStatus.Stopped;

        private NodeNetwork<string> Network { get; set; }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public async Task Start() {
            Network = new NodeNetwork<string>();

            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new MethodRegistrar<ServerMessagedEventArgs>(ProcessText, args => true, Commands.PRIVMSG, null), Name));
        }

        public async Task Stop() {
            if (Status.Equals(PluginStatus.Running) || Status.Equals(PluginStatus.Processing)) {
                await Log($"Stop called but process is still running from: {Name}");
            } else {
                await Log($"Plugin stopped: {Name}");
            }
        }

        public async Task CallDie() {
            Status = PluginStatus.Stopped;
            await Log($"Calling die, stopping processes —— plugin: {Name}");
        }

        public event AsyncEventHandler<PluginActionEventArgs> Callback;

        #endregion

        #region METHODS

        private async Task Log(params string[] args) {
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.Log, string.Join(" ", args), Name));
        }

        private async Task DoCallback(object source, PluginActionEventArgs args) {
            if (Callback == null) {
                return;
            }

            args.PluginName = Name;

            await Callback.Invoke(source, args);
        }

        #endregion
    }
}