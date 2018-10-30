using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Model;
using Convex.Client.Model.Log.Sinks;
using Convex.Client.Models.Proxy;
using Convex.Util;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Convex.Client.Services {
    public class IrcService : IHostedService, IIrcService {
        public IrcService(IIrcHubMethodsProxy ircHubMethodsProxy) {
            _ircHubMethodsProxy = ircHubMethodsProxy;
            IrcClientWrapper = new IrcClientWrapper(Program.Config);

            Address = "irc.foonetic.net";
            Port = 6667;
        }

        #region METHODS

        private async Task DoWork() {
            await IrcClientWrapper.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        private IIrcHubMethodsProxy _ircHubMethodsProxy;

        public IIrcClientWrapper IrcClientWrapper { get; }
        public string Address { get; }
        public int Port { get; }

        #endregion

        #region INIT

        public async Task Initialise() {
            await InitialiseClient();
        }

        private async Task InitialiseClient() {
            await IrcClientWrapper.Initialise(Address, Port);
        }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public async Task StartAsync(CancellationToken cancellationToken) {
            Log.Logger = new LoggerConfiguration().WriteTo.RollingFile(Program.Config.LogFilePath).WriteTo.ServerMessageSink(_ircHubMethodsProxy).CreateLogger();

            IrcClientWrapper.RegisterMethod(new Plugin.Registrar.MethodRegistrar<Event.ServerMessagedEventArgs>((args) => {
                _ircHubMethodsProxy.BroadcastMessage(StaticLog.Format(args.Message.Nickname, args.Message.Args));
                return Task.CompletedTask;
            }, null, Commands.ALL, null));

            await Initialise();
            await DoWork();
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Dispose();

            return Task.CompletedTask;
        }

        public void Dispose() {
            IrcClientWrapper.Dispose();
        }

        #endregion
    }
}