#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static readonly string GlobalConfigurationFilePath = $@"{ConfigurationsDirectory}/{GLOBAL_CONFIGURATION_FILE_NAME}";
        public static readonly string LogFilePath =$@"{LogsDirectory}/runtime-{DateTime.Now}.log"; 

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

            _AssemblyVersion = new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

            Log.Information($"Client is version {_AssemblyVersion}");
            TerminateSignaled += async (sender, args) => { await Dispose(true); };
        }


        #region IClient

        private readonly string _UniqueId;
        private readonly Version _AssemblyVersion;
        private bool _Initialized;

        private PluginHostWrapper<ServerMessagedEventArgs> _PluginHostWrapper;

        string IClient.UniqueId => _UniqueId;
        Version IClient.AssemblyVersion => _AssemblyVersion;
        bool IClient.Initialized => _Initialized;

        async Task<IInitializedClient> IClient.Initialize()
        {
            CheckForBasicDirectories();

            LoadGlobalConfiguration();

            _PluginHostWrapper = new PluginHostWrapper<ServerMessagedEventArgs>(_Configuration, OnInvokedMethod, "Convex.*.dll");
            _PluginHostWrapper.Terminated += OnTerminated;
            await _PluginHostWrapper.Load();

            _Initialized = _PluginHostWrapper.IsInitialized;

            if (_PluginHostWrapper.IsInitialized)
            {
                _Initialized = true;
                Log.Information("Client initialized.");
            }
            else
            {
                Log.Error("Client failed to initialize.");
            }

            await OnInitializationCompleted(this, new ClassInitializedEventArgs(this));

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

            Log.Information("Default global configuration created. Many values are empty by default, so you will have to edit the /config/global.conf file.");
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

        public event AsyncEventHandler<ClassInitializedEventArgs> InitializationCompleted;


        private async Task OnDatabaseQueried(object source, DatabaseQueriedEventArgs args)
        {
            if (DatabaseQueried == null)
            {
                return;
            }

            await DatabaseQueried.Invoke(source, args);
        }

        private async Task OnTerminated(object source, OperationTerminatedEventArgs args)
        {
            if (TerminateSignaled == null)
            {
                return;
            }

            await TerminateSignaled.Invoke(source, args);
        }

        private async Task OnInitializationCompleted(object source, ClassInitializedEventArgs args)
        {
            if (InitializationCompleted == null)
            {
                return;
            }

            await InitializationCompleted.Invoke(source, args);
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
            _Server = new Server(address);
            // todo change logic to not use events internally
            _Server.MessageReceived += OnMessageReceived;

            await _Server.Connection.Connect();

            await _Server.SendIdentityInfo(_Configuration[nameof(Core)]["Nickname"].StringValue, _Configuration[nameof(Core)]["Realname"].StringValue);
        }

        async Task IInitializedClient.BeginListenAsync()
        {
            do
            {
                await _Server.ListenAsync(this);
            } while (_Server.Connection.Connected);
        }

        private async Task OnMessageReceived(object source, ServerMessagedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Message.Command)
                || args.Message.Command.Equals(Commands.ERROR)
                || args.Message.Nickname.Equals(_Configuration[nameof(Core)]["Nickname"].StringValue)
                || _Configuration[nameof(Core)]["IgnoreList"].StringValueArray.Contains(args.Message.RealName))
            {
                return;
            }

            if (args.Message.SplitArgs.Count >= 2 && args.Message.SplitArgs[0].Equals(_Configuration[nameof(Core)]["Nickname"].StringValue.ToLower()))
            {
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();
            }

            await _PluginHostWrapper.Host.InvokeAsync(this, args);
        }

        private static async Task OnInvokedMethod(InvokedAsyncEventArgs<ServerMessagedEventArgs> args)
        {
            if (args.Args.Message.Command.Equals(Commands.ALL))
            {
                await InvokeSteps(args, Commands.ALL);
            }

            if (!args.Host.CompositionHandlers.ContainsKey(args.Args.Message.Command) || !args.Args.Execute)
            {
                return;
            }

            await InvokeSteps(args, args.Args.Message.Command);
        }

        /// <summary>
        ///     Step-invokes an InvokedAsyncEventArgs
        /// </summary>
        /// <param name="args">InvokedAsyncEventArgs object</param>
        /// <param name="contextCommand">Command to execute from</param>
        /// <returns></returns>
        private static async Task InvokeSteps(InvokedAsyncEventArgs<ServerMessagedEventArgs> args,
            string contextCommand)
        {
            foreach (IAsyncComposition<ServerMessagedEventArgs> composition in args.Host
                .CompositionHandlers[contextCommand].OrderBy(comp => comp.Priority))
            {
                if (!args.Args.Execute)
                {
                    return;
                }

                await composition.InvokeAsync(args.Args);
            }
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