#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition {
    public interface IAsyncCompsition<in T> {
        RegistrarExecutionStep ExecutionStep { get; }
        Predicate<T> CanExecute { get; }
        Func<T, Task> InnerMethod { get; }
        CompositionDescription Description { get; }
        string UniqueId { get; }
        string Command { get; }

        Task InvokeAsync(T args);
    }
}