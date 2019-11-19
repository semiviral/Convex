#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Core.Events;
using Convex.Core.Net;
using Convex.Core.Plugins.Compositions;
using Serilog;
using SharpConfig;

#endregion

namespace Convex.Core.Plugins
{
    public class PluginHost
    {
        public static readonly string PluginsDirectory = $@"{AppContext.BaseDirectory}/plugins/";

        public PluginHost(Configuration configuration, string pluginMask)
        {
            Configuration = configuration;
            PluginMask = pluginMask;
        }

        #region EVENTS

        public event AsyncEventHandler<PluginActionEventArgs> PluginCallback;

        #endregion

        #region MEMBERS

        private string PluginMask { get; }
        private Configuration Configuration { get; }

        private List<PluginInstance> Plugins { get; } = new List<PluginInstance>();

        public Dictionary<string, List<IAsyncComposition<ServerMessagedEventArgs>>> CompositionHandlers { get; } =
            new Dictionary<string, List<IAsyncComposition<ServerMessagedEventArgs>>>();

        public Dictionary<string, CompositionDescription> DescriptionRegistry { get; } =
            new Dictionary<string, CompositionDescription>();

        public bool ShuttingDown { get; private set; }

        #endregion

        #region METHODS

        public void StartPlugins()
        {
            foreach (PluginInstance pluginInstance in Plugins)
            {
                pluginInstance.Instance.Start(Configuration);
            }
        }

        public async Task StopPlugins()
        {
            if (Plugins.Count == 0)
            {
                return;
            }

            Log.Information("Stop plugins received; stopping plugins.");
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins)
            {
                await pluginInstance.Instance.Stop();
            }
        }

        public void RegisterComposition(IAsyncComposition<ServerMessagedEventArgs> composition)
        {
            try
            {
                AddComposition(composition);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load composition {composition.UniqueId}: {ex}");
            }

            if (DescriptionRegistry.ContainsKey(composition.UniqueId))
            {
                Log.Warning($"'{composition.UniqueId}' description already exists, skipping entry.");
            }
            else
            {
                DescriptionRegistry.Add(composition.UniqueId, composition.Description);
            }
        }

        private void AddComposition(IAsyncComposition<ServerMessagedEventArgs> composition)
        {
            foreach (string command in composition.Commands)
            {
                if (!CompositionHandlers.ContainsKey(command))
                {
                    CompositionHandlers.Add(command, new List<IAsyncComposition<ServerMessagedEventArgs>>());
                }

                CompositionHandlers[command].Add(composition);
            }
        }


        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public Task LoadPlugins()
        {
            try
            {
                foreach (string filePath in Directory.GetFiles(PluginsDirectory, PluginMask,
                    SearchOption.AllDirectories))
                {
                    foreach (IPlugin plugin in GetIPluginInstances(filePath))
                    {
                        Type pluginType = plugin.GetType();

                        foreach (MethodInfo methodInfo in pluginType.GetMethods(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            Composition composition = methodInfo.GetCustomAttribute<Composition>();

                            if (composition == null)
                            {
                                continue;
                            }

                            // local wrapper function
                            async Task Method(ServerMessagedEventArgs args)
                            {
                                await (Task)methodInfo.Invoke(pluginType, new object[] { args });
                            }

                            CompositionDescription description =
                                methodInfo.GetCustomAttribute<CompositionDescription>() ??
                                MethodComposition.UndefinedDescription;

                            MethodComposition<ServerMessagedEventArgs> methodComposition =
                                new MethodComposition<ServerMessagedEventArgs>(Method, composition, description);

                            RegisterComposition(methodComposition);
                        }

                        plugin.Callback += OnPluginCallback;
                        AddPlugin(plugin, false);
                    }
                }
            }
            catch (TypeLoadException)
            {
                // loading methodinfo failed, so just ignore error
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Error("LoaderException(s) occured loading a plugin:");

                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    Log.Error(loaderException.Message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"Error occurred loading a plugin: {ex}");
            }

            if (Plugins.Count > 0)
            {
                Log.Information(
                    $"Loaded plugins: {string.Join(", ", Plugins.Select(plugin => (plugin.Instance.Name, plugin.Instance.Version)))}");
            }

            return Task.CompletedTask;
        }


        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetIPluginInstances(string assemblyName)
        {
            foreach (Type type in GetAssembly(assemblyName).GetTypes())
            {
                if (!type.IsAssignableFrom(typeof(IPlugin)))
                {
                    continue;
                }

                yield return (IPlugin)Activator.CreateInstance(type);
            }
        }

        private static Assembly GetAssembly(string assemblyName)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
        }

        /// <summary>
        ///     Adds IPlugin instance to internal list
        /// </summary>
        /// <param name="plugin">plugin instance</param>
        /// <param name="autoStart">start plugin immediately</param>
        private void AddPlugin(IPlugin plugin, bool autoStart)
        {
            try
            {
                Plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart)
                {
                    plugin.Start(Configuration);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding plugin: {ex.Message}");
            }
        }

        public async Task InvokeAsync(ServerMessagedEventArgs args)
        {
            if (!CompositionHandlers.ContainsKey(args.Message.Command) || !args.Execute)
            {
                return;
            }

            await InvokeSteps(args);
        }

        /// <summary>
        ///     Step-invokes an InvokedAsyncEventArgs
        /// </summary>
        /// <param name="args">InvokedAsyncEventArgs object</param>
        /// <param name="contextCommand">Command to execute from</param>
        /// <returns></returns>
        private async Task InvokeSteps(ServerMessagedEventArgs args)
        {
            foreach (IAsyncComposition<ServerMessagedEventArgs> composition in CompositionHandlers[args.Message.Command]
                .OrderBy(comp => comp.Priority))
            {
                await composition.InvokeAsync(args);
            }
        }

        private async Task OnPluginCallback(object sender, PluginActionEventArgs args)
        {
            if (PluginCallback == null)
            {
                return;
            }

            await PluginCallback.Invoke(sender, args);
        }
    }

    #endregion
}