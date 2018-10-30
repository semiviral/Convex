#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;
using Convex.Plugin.Registrar;
using Convex.Util;
using Serilog.Events;

#endregion

namespace Convex.Plugin {
    public class PluginHost<T> where T : EventArgs {
        public PluginHost(string pluginsDirectory, Func<T, Task> onInvokedMethod) {
            PluginsDirectory = pluginsDirectory;
            OnInvoked = onInvokedMethod;
        }

        #region MEMBERS

        private const string _PluginMask = "Convex.*.dll";

        private List<PluginInstance> Plugins { get; } = new List<PluginInstance>();

        public Dictionary<string, AsyncEventHandler<T>> CompositionHandlers { get; } = new Dictionary<string, AsyncEventHandler<T>>();
        public Dictionary<string, Tuple<string, string>> DescriptionRegistry { get; } = new Dictionary<string, Tuple<string, string>>();

        public bool ShuttingDown { get; private set; }
        public Func<T, Task> OnInvoked { get; }
        public string PluginsDirectory { get; }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<PluginActionEventArgs> PluginCallback;

        #endregion

        #region METHODS

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in Plugins) {
                pluginInstance.Instance.Start();
            }
        }

        public async Task StopPlugins() {
            StaticLog.Log(new LogEventArgs(LogEventLevel.Information, "STOP PLUGINS RECEIVED — shutting down."));
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins) {
                await pluginInstance.Instance.Stop();
            }
        }

        public async Task InvokeAsync(T args) {
            await OnInvoked(args);
        }

        public void RegisterMethod(IAsyncRegistrar<T> registrar) {
            AddComposition(registrar);

            if (DescriptionRegistry.Keys.Contains(registrar.UniqueId)) {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information, $"'{registrar.UniqueId}' description already exists, skipping entry."));
            } else {
                DescriptionRegistry.Add(registrar.UniqueId, registrar.Description);
            }
        }

        private void AddComposition(IAsyncRegistrar<T> registrar) {
            if (!CompositionHandlers.ContainsKey(registrar.Command)) {
                CompositionHandlers.Add(registrar.Command, null);
            }

            CompositionHandlers[registrar.Command] += async (sender, args) => {
                if (!registrar.CanExecute(args)) {
                    return;
                }

                await registrar.Composition(args);
            };
        }


        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public Task LoadPlugins() {
            if (!Directory.Exists(PluginsDirectory)) {
                Directory.CreateDirectory(PluginsDirectory);
            }

            IPlugin currentPluginIterated = null;

            try {
                // array of all filepaths that are found to match the PLUGIN_MASK
                IEnumerable<IPlugin> pluginInstances = Directory
                    .GetFiles(PluginsDirectory, _PluginMask, SearchOption.AllDirectories)
                    .SelectMany(GetPluginInstances);

                foreach (IPlugin plugin in pluginInstances) {
                    currentPluginIterated = plugin;

                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            } catch (ReflectionTypeLoadException ex) {
                foreach (Exception loaderException in ex.LoaderExceptions) {
                    StaticLog.Log(new LogEventArgs(LogEventLevel.Information, $"LoaderException occured loading a plugin: {loaderException}"));
                }
            } catch (Exception ex) {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information, $"Error occurred loading a plugin ({currentPluginIterated.Name}, {currentPluginIterated.Version}): {ex}"));
            }

            if (Plugins.Count > 0) {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Information, $"Loaded plugins: {string.Join(", ", Plugins.Select(plugin => new Tuple<string, Version>(plugin.Instance.Name, plugin.Instance.Version)))}"));
            }

            return Task.CompletedTask;
        }


        /// <summary>
        ///     Gets instance of plugin by assembly name
        /// </summary>
        /// <param name="assemblyName">full name of assembly</param>
        /// <returns></returns>
        private static IEnumerable<IPlugin> GetPluginInstances(string assemblyName) {
            return GetTypeInstances(GetAssembly(assemblyName)).Select(type => (IPlugin)Activator.CreateInstance(type));
        }

        /// <summary>
        ///     Gets the IPlugin type instance from an assembly name
        /// </summary>
        /// <param name="assembly">assembly instance</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypeInstances(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetTypeInfo().GetInterfaces().Contains(typeof(IPlugin)));
        }

        private static Assembly GetAssembly(string assemblyName) {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
        }

        /// <summary>
        ///     Adds IPlugin instance to internal list
        /// </summary>
        /// <param name="plugin">plugin instance</param>
        /// <param name="autoStart">start plugin immediately</param>
        private void AddPlugin(IPlugin plugin, bool autoStart) {
            try {
                Plugins.Add(new PluginInstance(plugin, PluginStatus.Stopped));

                if (autoStart) {
                    plugin.Start();
                }
            } catch (Exception ex) {
                StaticLog.Log(new LogEventArgs(LogEventLevel.Warning, $"Error adding plugin: {ex.Message}"));
            }
        }

        private async Task OnPluginCallback(object source, PluginActionEventArgs e) {
            if (PluginCallback == null) {
                return;
            }

            await PluginCallback.Invoke(source, e);
        }
    }

    #endregion
}