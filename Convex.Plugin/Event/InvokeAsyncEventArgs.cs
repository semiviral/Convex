using System;

namespace Convex.Plugin.Event {
    public class InvokeAsyncEventArgs<T> : EventArgs where T : EventArgs {
        public InvokeAsyncEventArgs(PluginHost<T> host, T args) {
            Host = host;
            Args = args;
        }

        #region MEMBERS

        public PluginHost<T> Host { get; }
        public T Args { get; set; }

        #endregion
    }
}
