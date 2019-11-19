#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Convex.Core.Events;
using Convex.Core.Net;
using Convex.Core.Plugins;
using Convex.Core.Plugins.Compositions;
using Serilog;
using SharpConfig;

#endregion

namespace Convex.Core
{
    public sealed class Client : IClient, IInitializedClient
    {
        public const string GLOBAL_CONFIGURATION_FILE_NAME = "global.conf";

        public static readonly string ConfigurationsDirectory = $@"{AppContext.BaseDirectory}/config/";
        public static readonly string LogsDirectory = $@"{AppContext.BaseDirectory}/logs/";

        public static readonly string GlobalConfigurationFilePath =
            $@"{ConfigurationsDirectory}/{GLOBAL_CONFIGURATION_FILE_NAME}";

        public static readonly string LogFilePath = $@"{LogsDirectory}/runtime-{DateTime.Now}.log";

        public event AsyncEventHandler<ServerMessagedEventArgs> ServerMessaged; 

        /// <summary>
        ///     Initializes class. No connections are made at initialization, so call `Load()` to begin sending and
        ///     receiving.
        /// </summary>
        public Client()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(conf => conf.Console())
                .WriteTo.Async(conf => conf.RollingFile(LogFilePath))
                .CreateLogger();

            Log.Information("Default logger created.");

            _UniqueId = Guid.NewGuid().ToString();

            Log.Information($"Client UniqueID is: {_UniqueId}");

            _AssemblyVersion = GetType().Assembly.GetName().Version ?? new Version(string.Empty);

            Log.Information($"Client is version {_AssemblyVersion}");
            TerminateSignaled += async (sender, args) => { await Dispose(true); };
        }

        private async Task OnServerMessaged(object sender, ServerMessagedEventArgs args)
        {
            if (ServerMessaged == null)
            {
                return;
            }

            await ServerMessaged.Invoke(sender, args);
        }


        #region IClient

        private readonly string _UniqueId;
        private readonly Version _AssemblyVersion;
        private bool _Initialized;

        private PluginHostWrapper _PluginHostWrapper;

        string IClient.UniqueId => _UniqueId;
        Version IClient.AssemblyVersion => _AssemblyVersion;
        bool IClient.Initialized => _Initialized;

        IInitializedClient IClient.InitializedClient
        {
            get
            {
                if (!_Initialized)
                {
                    throw new Exception(
                        "Client is not yet initialized. Please initialize client before trying to access.");
                }

                return this;
            }
        }

        async Task<IInitializedClient> IClient.Initialize()
        {
            CheckForBasicDirectories();

            LoadGlobalConfiguration();

            _PluginHostWrapper = new PluginHostWrapper(_Configuration, "Convex.*.dll");
            _PluginHostWrapper.Terminated += OnTerminated;
            await _PluginHostWrapper.Load();

            _Server = new Server();

            Log.Information("Client successfully initialized.");
            _Initialized = true;

            return this;
        }

        private static void CheckForBasicDirectories()
        {
            if (!Directory.Exists(ConfigurationsDirectory))
            {
                Log.Information("Configurations directory missing; creating.");
                Directory.CreateDirectory(ConfigurationsDirectory);
            }

            if (!Directory.Exists(PluginHost.PluginsDirectory))
            {
                Log.Information("Plugins directory missing; creating.");
                Directory.CreateDirectory(PluginHost.PluginsDirectory);
            }

            if (!Directory.Exists(LogsDirectory))
            {
                Log.Information("Logs directory missing; creating.");
                Directory.CreateDirectory(LogsDirectory);
            }
        }

        private void CreateGlobalConfiguration()
        {
            _Configuration = new Configuration
            {
                nameof(Core)
            };

            _Configuration[nameof(Core)]["Nickname"].StringValue = string.Empty;
            _Configuration[nameof(Core)]["Realname"].StringValue = string.Empty;
            _Configuration[nameof(Core)]["IgnoreList"].StringValueArray = new string[0];

            _Configuration.SaveToFile(GlobalConfigurationFilePath);

            Log.Information(
                "Default global configuration created. Many values are empty by default, so you will have to edit the /config/global.conf file.");
        }

        private void LoadGlobalConfiguration()
        {
            if (!File.Exists(GlobalConfigurationFilePath))
            {
                Log.Information("Configuration file not found. Creating default.");

                CreateGlobalConfiguration();
            }

            _Configuration = Configuration.LoadFromFile(GlobalConfigurationFilePath);
            //IgnoreList =
            //    new ObservableCollection<string>(Configuration[nameof(Core)][nameof(IgnoreList)].StringValueArray
            //                                     ?? new string[0]);
            //IgnoreList.CollectionChanged += (sender, args) =>
            //{
            //    // save ignore list on changes
            //    Configuration[nameof(Core)][nameof(IgnoreList)].StringValueArray = IgnoreList.ToArray();
            //    Configuration.SaveToFile(GlobalConfigurationFilePath);
            //};
        }

        void IClient.RegisterMethod(IAsyncComposition<ServerMessagedEventArgs> methodRegistrar)
        {
            _PluginHostWrapper.Host.RegisterComposition(methodRegistrar);
        }

        public event AsyncEventHandler<DatabaseQueriedEventArgs> DatabaseQueried;

        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;


        private async Task OnDatabaseQueried(object sender, DatabaseQueriedEventArgs args)
        {
            if (DatabaseQueried == null)
            {
                return;
            }

            await DatabaseQueried.Invoke(sender, args);
        }

        private async Task OnTerminated(object sender, OperationTerminatedEventArgs args)
        {
            if (TerminateSignaled == null)
            {
                return;
            }

            await TerminateSignaled.Invoke(sender, args);
        }

        #endregion


        #region IInitializedClient

        private Configuration _Configuration;
        private Server _Server;

        Configuration IInitializedClient.Configuration => _Configuration;
        Server IInitializedClient.Server => _Server;

        IReadOnlyDictionary<string, CompositionDescription> IInitializedClient.CompositionDescriptions =>
            _PluginHostWrapper.Host.DescriptionRegistry;


        async Task IInitializedClient.Connect(IAddress address)
        {
            // todo implement server messaged event for consuming applications / non-plugin assemblies
            _Server.Connection.DataReceived += OnDataReceived;

            await _Server.Connection.Connect(address);
            await _Server.SendIdentityInfo(_Configuration[nameof(Core)]["Nickname"].StringValue,
                _Configuration[nameof(Core)]["Realname"].StringValue);
        }

        /// <summary>
        ///     Processes received raw stream data into a ServerMessage and invoked the current plugin host.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async Task OnDataReceived(object sender, StreamDataEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            if (CheckIsPing(args.Data))
            {
                await ReturnPing(args.Data);
                return;
            }

            ServerMessage serverMessage = new ServerMessage(args.Data);

            if (string.IsNullOrEmpty(serverMessage.Command)
                || serverMessage.Command.Equals(Commands.ERROR)
                || serverMessage.Nickname.Equals(_Configuration[nameof(Core)]["Nickname"].StringValue)
                || _Configuration[nameof(Core)]["IgnoreList"].StringValueArray.Contains(serverMessage.RealName))
            {
                return;
            }

            if (serverMessage.SplitArgs.Count >= 2 && serverMessage.SplitArgs[0]
                    .Equals(_Configuration[nameof(Core)]["Nickname"].StringValue.ToLower()))
            {
                serverMessage.InputCommand = serverMessage.SplitArgs[1].ToLower();
            }
            
            ServerMessagedEventArgs serverMessagedEventArgs = new ServerMessagedEventArgs(this, serverMessage);

            // Invoke ServerMessaged event
            await OnServerMessaged(this, serverMessagedEventArgs);

            // Invoke PluginHost
            await _PluginHostWrapper.Host.InvokeAsync(serverMessagedEventArgs);
        }

        /// <summary>
        ///     Check whether the data received is a ping message and reply
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private static bool CheckIsPing(string rawData)
        {
            return rawData.StartsWith(Commands.PING);
        }

        private async Task ReturnPing(string rawData)
        {
            string pingData = rawData.Remove(0, 5); // removes 'PING ' from string
            await _Server.Connection.EstablishedConnection.SendCommandAsync(new CommandEventArgs(Commands.PONG, pingData));
        }

        #endregion


        #region IDisposable

        private bool _Disposed;

        /// <summary>
        ///     Dispose of all streams and objects
        /// </summary>
        public void Dispose()
        {
            Dispose(true).Wait();
        }

        private async Task Dispose(bool dispose)
        {
            if (!dispose || _Disposed)
            {
                return;
            }

            await _PluginHostWrapper.Host.StopPlugins();
            _Server?.Dispose();
            Log.CloseAndFlush();

            _Disposed = true;
        }

        #endregion
    }
}