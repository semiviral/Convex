#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convex.Core.Net;
using Convex.Event;
using Convex.Plugin;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Serilog;
using SharpConfig;

#endregion

namespace Convex.Core
{
    public sealed class Client : IDisposable, IClient
    {
        public const string GLOBAL_CONFIGURATION_FILE_NAME = "global.conf";

        public static readonly string ConfigurationsDirectory = $@"{AppContext.BaseDirectory}/config/";
        public static readonly string LogsDirectory = $@"{AppContext.BaseDirectory}/logs/";

        public static readonly string GlobalConfigurationFilePath =
            $@"{ConfigurationsDirectory}/{GLOBAL_CONFIGURATION_FILE_NAME}";

        public static readonly string LogFilePath =
            $@"{LogsDirectory}/runtime-{DateTime.Now}.log";

        public Configuration Configuration { get; private set; }

        #region CONFIGURATION PROPERTIES

        public string Nickname
        {
            get => Configuration[nameof(Core)][nameof(Nickname)].StringValue;
            set
            {
                Configuration[nameof(Core)][nameof(Nickname)].SetValue(value);
                Configuration.SaveToFile(GlobalConfigurationFilePath);
            }
        }

        public string Realname
        {
            get => Configuration[nameof(Core)][nameof(Nickname)].StringValue;
            set
            {
                Configuration[nameof(Core)][nameof(Nickname)].SetValue(value);
                Configuration.SaveToFile(GlobalConfigurationFilePath);
            }
        }

        public ObservableCollection<string> IgnoreList { get; private set; }

        #endregion

        /// <summary>
        ///     Initializes class. No connections are made at initialization, so call `Initialize()` to begin sending and
        ///     receiving.
        /// </summary>
        public Client()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(conf => conf.Console())
                .WriteTo.Async(conf => conf.RollingFile(LogFilePath))
                .CreateLogger();

            Version = new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;

            _PendingPlugins = new Stack<IAsyncComposition<ServerMessagedEventArgs>>();

            UniqueId = Guid.NewGuid();
            Server = new Server();

            TerminateSignaled += Terminate;
            Server.MessageReceived += OnMessageReceived;

            PluginHostWrapper =
                new PluginHostWrapper<ServerMessagedEventArgs>(Configuration, OnInvokedMethod, "Convex.*.dll");
            PluginHostWrapper.TerminateSignaled += OnTerminateSignaled;
            PluginHostWrapper.CommandReceived += Server.Connection.SendDataAsync;
        }

        #region INTERFACE IMPLEMENTATION

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

            await PluginHostWrapper.Host.StopPlugins();
            Server?.Dispose();
            Log.CloseAndFlush();

            _Disposed = true;
        }

        #endregion

        #region MEMBERS

        public Guid UniqueId { get; }
        public Server Server { get; }
        public Version Version { get; }

        public IReadOnlyDictionary<string, CompositionDescription> PluginCommands =>
            PluginHostWrapper.Host.DescriptionRegistry;

        public string Address => Server.Connection.Address.Hostname;
        public int Port => Server.Connection.Address.Port;
        public bool IsInitialized { get; private set; }
        public bool Initializing { get; private set; }

        private PluginHostWrapper<ServerMessagedEventArgs> PluginHostWrapper { get; }

        private bool _Disposed;

        private readonly Stack<IAsyncComposition<ServerMessagedEventArgs>> _PendingPlugins;

        #endregion

        #region RUNTIME

        public async Task BeginListenAsync()
        {
            do
            {
                await Server.ListenAsync(this);
            } while (Server.Connection.IsConnected);
        }

        private async Task OnMessageReceived(object source, ServerMessagedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Message.Command))
            {
                return;
            }

            if (args.Message.Command.Equals(Commands.ERROR))
            {
                return;
            }

            if (args.Message.Nickname.Equals(Nickname)
                || IgnoreList.Contains(args.Message.RealName))
            {
                return;
            }

            if ((args.Message.SplitArgs.Count >= 2) && args.Message.SplitArgs[0].Equals(Nickname.ToLower()))
            {
                args.Message.InputCommand = args.Message.SplitArgs[1].ToLower();
            }

            try
            {
                await PluginHostWrapper.Host.InvokeAsync(this, args);
            }
            catch (Exception ex)
            {
                await OnError(this, new ErrorEventArgs(ex));
            }
        }
        
        private static async Task OnInvokedMethod(InvokedAsyncEventArgs<ServerMessagedEventArgs> args)
        {
            if (!args.Args.Message.Command.Equals(Commands.ALL))
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

        #region INIT

        public async Task<bool> Initialize(IAddress address)
        {
            if (IsInitialized || Initializing)
            {
                return true;
            }

            Initializing = true;

            CheckForBasicDirectories();

            InitializeGlobalConfiguration();

            await PluginHostWrapper.Initialize();

            RegisterMethods();

            await Server.Initialize(address);

            if (!Server.IsInitialized)
            {
                Log.Error("Client failed to initialize.");
                return false;
            }

            await OnInitialized(this, new ClassInitializedEventArgs(this));

            await Server.SendIdentityInfo(Nickname, Realname);

            Initializing = false;

            if (!PluginHostWrapper.IsInitialized)
            {
                Log.Error("Client failed to initialize.");
                return false;
            }

            IsInitialized = Server.IsInitialized && PluginHostWrapper.IsInitialized;
            Log.Information("Client initialized.");
            return true;
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

        private void InitializeGlobalConfiguration()
        {
            if (!File.Exists(GlobalConfigurationFilePath))
            {
                Log.Information("Configuration file not found. Creating default.");

                CreateDefaultGlobalConfiguration();
            }

            Configuration = Configuration.LoadFromFile(GlobalConfigurationFilePath);
            IgnoreList =
                new ObservableCollection<string>(Configuration[nameof(Core)][nameof(IgnoreList)].StringValueArray
                                                 ?? new string[0]);
            IgnoreList.CollectionChanged += (sender, args) =>
            {
                // save ignore list on changes
                Configuration[nameof(Core)][nameof(IgnoreList)].StringValueArray = IgnoreList.ToArray();
                Configuration.SaveToFile(GlobalConfigurationFilePath);
            };
        }

        private void CreateDefaultGlobalConfiguration()
        {
            Configuration = new Configuration
            {
                nameof(Core)
            };

            Configuration[nameof(Core)][nameof(Nickname)].StringValue = string.Empty;
            Configuration[nameof(Core)][nameof(Realname)].StringValue = string.Empty;
            Configuration[nameof(Core)][nameof(IgnoreList)].StringValueArray = new string[0];

            Configuration.SaveToFile(GlobalConfigurationFilePath);

            Log.Information(
                "Default global configuration created. Many values are empty by default, so you will have to edit the /config/global.conf file.");
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<DatabaseQueriedEventArgs> Queried;
        public event AsyncEventHandler<OperationTerminatedEventArgs> TerminateSignaled;
        public event AsyncEventHandler<ClassInitializedEventArgs> Initialized;
        public event AsyncEventHandler<ErrorEventArgs> Error;

        private async Task OnQuery(object source, DatabaseQueriedEventArgs args)
        {
            if (Queried == null)
            {
                return;
            }

            await Queried.Invoke(source, args);
        }

        private async Task OnTerminateSignaled(object source, OperationTerminatedEventArgs args)
        {
            if (TerminateSignaled == null)
            {
                return;
            }

            await TerminateSignaled.Invoke(source, args);
        }

        private async Task OnInitialized(object source, ClassInitializedEventArgs args)
        {
            if (Initialized == null)
            {
                return;
            }

            await Initialized.Invoke(source, args);
        }

        private async Task OnError(object source, ErrorEventArgs args)
        {
            if (Error == null)
            {
                return;
            }

            await Error.Invoke(source, args);
        }

        private async Task Terminate(object source, OperationTerminatedEventArgs args)
        {
            await Dispose(true);
        }

        #endregion

        #region METHODS

        public void RegisterMethod(IAsyncComposition<ServerMessagedEventArgs> methodRegistrar)
        {
            if (!IsInitialized && !Initializing)
            {
                _PendingPlugins.Push(methodRegistrar);

                return;
            }

            PluginHostWrapper.Host.RegisterComposition(methodRegistrar);
        }

        private void RegisterMethods()
        {
            while (_PendingPlugins.Count > 0)
            {
                RegisterMethod(_PendingPlugins.Pop());
            }
        }

        /// <summary>
        ///     Returns a specified command from commands list
        /// </summary>
        /// <param name="command">Command to be returned</param>
        /// <returns></returns>
        public CompositionDescription GetDescription(string command)
        {
            return PluginCommands.Single(kvp =>
                kvp.Value.Command.Equals(command, StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        public bool TryGetDescription(string command, out CompositionDescription compositionDescription)
        {
            compositionDescription = PluginCommands.SingleOrDefault(kvp =>
                kvp.Value.Command.Equals(command, StringComparison.CurrentCultureIgnoreCase)).Value;

            return compositionDescription == default;
        }

        /// <summary>
        ///     Checks whether specified comamnd exists
        /// </summary>
        /// <param name="command">comamnd name to be checked</param>
        /// <returns>True: exists; false: does not exist</returns>
        public bool CommandExists(string command) => TryGetDescription(command, out CompositionDescription _);

        #endregion
    }
}
