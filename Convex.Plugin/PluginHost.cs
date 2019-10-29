#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Convex.Util;
using Serilog.Events;

#endregion

namespace Convex.Plugin
{
    public class PluginHost<T> where T : EventArgs
    {
        public PluginHost(string pluginsDirectory, Func<InvokedAsyncEventArgs<T>, Task> invokeAsyncMethod,
            string pluginMask)
        {
            PluginsDirectory = pluginsDirectory;
            InvokedAsync += async (source, args) => await invokeAsyncMethod(new InvokedAsyncEventArgs<T>(this, args));
            PluginMask = pluginMask;
        }

        #region MEMBERS

        private string PluginMask { get; }

        private List<PluginInstance> Plugins { get; } = new List<PluginInstance>();

        public Dictionary<string, List<IAsyncCompsition<T>>> CompositionHandlers { get; } =
            new Dictionary<string, List<IAsyncCompsition<T>>>();

        public Dictionary<string, CompositionDescription> DescriptionRegistry { get; } =
            new Dictionary<string, CompositionDescription>();

        public bool ShuttingDown { get; private set; }
        public string PluginsDirectory { get; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<T> InvokedAsync;
        public event AsyncEventHandler<PluginActionEventArgs> PluginCallback;

        #endregion

        #region METHODS

        public void StartPlugins()
        {
            foreach (PluginInstance pluginInstance in Plugins)
            {
                pluginInstance.Instance.Start();
            }
        }

        public async Task StopPlugins()
        {
            StaticLog.Log(new LogEventArgs(LogEventLevel.Information, "STOP PLUGINS RECEIVED — shutting down."));
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins)
            {
                await pluginInstance.Instance.Stop();
            }
        }

        public void RegisterComposition(IAsyncCompsition<T> composition)
        {
            try
            {
                AddComposition(composition);
            }
            catch (Exception ex)
            {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Error,
                    $"Failed to load composition {composition.UniqueId}: {ex}"));
            }

            if (DescriptionRegistry.Keys.Contains(composition.UniqueId))
            {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information,
                    $"'{composition.UniqueId}' description already exists, skipping entry."));
            }
            else
            {
                DescriptionRegistry.Add(composition.UniqueId, composition.Description);
            }

            StaticLog.Log(new LogEventArgs(LogEventLevel.Information, $"Loaded plugin: {composition.UniqueId}"));
        }

        private void AddComposition(IAsyncCompsition<T> composition)
        {
            foreach (string command in composition.Commands)
            {
                if (!CompositionHandlers.ContainsKey(command))
                {
                    CompositionHandlers.Add(command, new List<IAsyncCompsition<T>>());
                }

                CompositionHandlers[command].Add(composition);
            }
        }


        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public Task LoadPlugins()
        {
            if (!Directory.Exists(PluginsDirectory))
            {
                Directory.CreateDirectory(PluginsDirectory);
            }

            IPlugin currentPluginIterated = null;

            try
            {
                // array of all filepaths that are found to match the PLUGIN_MASK
                IEnumerable<IPlugin> pluginInstances = Directory
                    .GetFiles(PluginsDirectory, PluginMask, SearchOption.AllDirectories)
                    .SelectMany(GetPluginInstances);

                foreach (IPlugin plugin in pluginInstances)
                {
                    currentPluginIterated = plugin;

                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    StaticLog.Log(new LogEventArgs(LogEventLevel.Information,
                        $"LoaderException occured loading a plugin: {loaderException}"));
                }
            }
            catch (Exception ex)
            {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information,
                    $"Error occurred loading a plugin ({currentPluginIterated.Name}, {currentPluginIterated.Version}): {ex}"));
            }

            if (Plugins.Count > 0)
            {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information,
                    $"Loaded plugins: {string.Join(", ", Plugins.Select(plugin => new Tuple<string, Version>(plugin.Instance.Name, plugin.Instance.Version)))}"));
            }

            return Task.CompletedTask;
        }


        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetPluginInstances(string assemblyName)
        {
            return GetTypeInstances(GetAssembly(assemblyName)).Select(type => (IPlugin)Activator.CreateInstance(type));
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assembly">assembly instance</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypeInstances(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetTypeInfo().GetInterfaces().Contains(typeof(IPlugin)));
        }

        private static Assembly GetAssembly(string assemblyName) =>
            AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);

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
                    plugin.Start();
                }
            }
            catch (Exception ex)
            {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Warning, $"Error adding plugin: {ex.Message}"));
            }
        }

        public async Task InvokeAsync(object source, T args)
        {
            if (InvokedAsync == null)
            {
                return;
            }

            await InvokedAsync.Invoke(source, args);
        }

        private async Task OnPluginCallback(object source, PluginActionEventArgs e)
        {
            if (PluginCallback == null)
            {
                return;
            }

            await PluginCallback.Invoke(source, e);
        }
    }

    #endregion
}
