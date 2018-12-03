#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition {
    public interface IAsyncCompsition<in T> {
        int ExecutionStep { get; }
        Predicate<T> CanExecute { get; }
        Func<T, Task> InnerMethod { get; }
        CompositionDescription Description { get; }
        string UniqueId { get; }
        string[] Commands { get; }

        Task InvokeAsync(T args);
    }
}