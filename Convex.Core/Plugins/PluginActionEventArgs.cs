#region

using System;

#endregion

namespace Convex.Core.Plugins
{
    public class PluginActionEventArgs : EventArgs
    {
        public PluginActionEventArgs(PluginActionType actionType, object result, string pluginName)
        {
            Result = result;
            ActionType = actionType;
            PluginName = pluginName;
        }

        #region MEMBERS

        public PluginActionType ActionType { get; }
        public object Result { get; }
        public string PluginName { get; set; }

        #endregion
    }
}