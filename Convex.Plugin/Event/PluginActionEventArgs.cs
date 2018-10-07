#region usings

using System;

#endregion

namespace Convex.Plugin.Event {
    public class PluginActionEventArgs : EventArgs {
        #region MEMBERS

        public PluginActionType ActionType { get; }
        public object Result { get; }
        public string PluginName { get; set; }

        #endregion

        public PluginActionEventArgs(PluginActionType actionType, object result, string pluginName) {
            Result = result;
            ActionType = actionType;
            PluginName = pluginName;
        }
    }
}
