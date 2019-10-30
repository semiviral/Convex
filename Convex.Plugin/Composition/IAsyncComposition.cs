#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition
{
    public interface IAsyncComposition<in T>
    {
        int Priority { get; }
        Predicate<T> CanExecute { get; }
        Func<T, Task> Method { get; }
        CompositionDescription Description { get; }
        string UniqueId { get; }
        string[] Commands { get; }

        Task InvokeAsync(T args);
    }
}
