#region

using System;

#endregion

namespace Convex.Plugin.Event
{
    public class InvokedAsyncEventArgs<T> : EventArgs where T : EventArgs
    {
        public InvokedAsyncEventArgs(PluginHost<T> host, T args)
        {
            Host = host;
            Args = args;
        }

        #region MEMBERS

        public PluginHost<T> Host { get; }
        public T Args { get; set; }

        #endregion
    }
}
