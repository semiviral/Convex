#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Core.Plugins.Compositions
{
    public interface IAsyncComposition<in T>
    {
        string UniqueId { get; }
        int Priority { get; }
        Func<T, Task> Method { get; }
        CompositionDescription Description { get; }
        string[] Commands { get; }

        Task InvokeAsync(T args);
    }
}
