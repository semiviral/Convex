#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Convex.Event;
using Convex.Plugin.Event;
using Convex.Plugin.Registrar;

#endregion

namespace Convex.Plugin {
    public class PluginHost<T> where T : EventArgs {
        #region MEMBERS

        private const string _PluginMask = "Convex.*.dll";

        private List<PluginInstance> Plugins { get; } = new List<PluginInstance>();

        public Dictionary<string, AsyncEventHandler<T>> CompositionHandlers { get; } = new Dictionary<string, AsyncEventHandler<T>>();
        public Dictionary<string, Tuple<string, string>> DescriptionRegistry { get; } = new Dictionary<string, Tuple<string, string>>();

        public bool ShuttingDown { get; private set; }
        public Func<T, Task> OnInvoked { get; }
        public string PluginsDirectory { get; }

        #endregion

        public PluginHost(string pluginsDirectory, Func<T, Task> onInvokedMethod) {
            PluginsDirectory = pluginsDirectory;
            OnInvoked = onInvokedMethod;
        }

        #region EVENTS

        public event AsyncEventHandler<PluginActionEventArgs> PluginCallback;
        public event AsyncEventHandler<InformationLoggedEventArgs> Logged;

        private async Task OnLog(object sender, InformationLoggedEventArgs args) {
            if (Logged == null)
                return;

            await Logged.Invoke(sender, args);
        }

        #endregion

        #region METHODS

        public void StartPlugins() {
            foreach (PluginInstance pluginInstance in Plugins)
                pluginInstance.Instance.Start();
        }

        public async Task StopPlugins() {
            await OnLog(this, new InformationLoggedEventArgs("STOP PLUGINS RECIEVED — shutting down."));
            ShuttingDown = true;

            foreach (PluginInstance pluginInstance in Plugins)
                await pluginInstance.Instance.Stop();
        }

        public async Task InvokeAsync(T args) {
            await OnInvoked(args);
        }

        public void RegisterMethod(IAsyncRegistrar<T> registrar) {
            AddComposition(registrar);

            if (DescriptionRegistry.Keys.Contains(registrar.UniqueId))
                Debug.WriteLine($"'{registrar.UniqueId}' description already exists, skipping entry.");
            else
                DescriptionRegistry.Add(registrar.UniqueId, registrar.Description);
        }

        private void AddComposition(IAsyncRegistrar<T> registrar) {
            if (!CompositionHandlers.ContainsKey(registrar.Command))
                CompositionHandlers.Add(registrar.Command, null);

            CompositionHandlers[registrar.Command] += async (sender, args) => {
                if (!registrar.CanExecute(args))
                    return;

                await registrar.Composition(args);
            };
        }


        /// <summary>
        ///     Loads all plugins
        /// </summary>
        public async Task LoadPlugins() {
            if (!Directory.Exists(PluginsDirectory))
                Directory.CreateDirectory(PluginsDirectory);

            try {
                // array of all filepaths that are found to match the PLUGIN_MASK
                IEnumerable<IPlugin> pluginInstances = Directory.GetFiles(PluginsDirectory, _PluginMask, SearchOption.AllDirectories).SelectMany(GetPluginInstances);


                foreach (IPlugin plugin in pluginInstances) {
                    plugin.Callback += OnPluginCallback;
                    AddPlugin(plugin, false);
                }
            } catch (ReflectionTypeLoadException ex) {
                foreach (Exception loaderException in ex.LoaderExceptions)
                    await OnLog(this, new InformationLoggedEventArgs($"LoaderException occured loading a plugin: {loaderException}"));
            } catch (Exception ex) {
                await OnLog(this, new InformationLoggedEventArgs($"Error occured loading a plugin: {ex}"));
            }

            await OnLog(this, new InformationLoggedEventArgs($"Loaded plugins: {string.Join(", ", Plugins.Select(plugin => plugin.Instance.Name))}"));
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

                if (autoStart)
                    plugin.Start();
            } catch (Exception ex) {
                Debug.WriteLine(ex, $"Error adding plugin: {ex.Message}");
            }
        }

        private async Task OnPluginCallback(object source, PluginActionEventArgs e) {
            if (PluginCallback == null)
                return;

            await PluginCallback.Invoke(source, e);
        }
    }

    #endregion
}
