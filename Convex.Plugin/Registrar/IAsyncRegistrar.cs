#region usings

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Registrar {
    public interface IAsyncRegistrar<in T> {
        #region MEMBERS

        Predicate<T> CanExecute { get; }
        Func<T, Task> Composition { get; }
        Tuple<string, string> Description { get; }
        string UniqueId { get; }
        string Command { get; }
        bool IsRegistered { get; }

        #endregion
    }
}
