#region usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition {
    public class Composition<TEventArgs> : IAsyncCompsition<TEventArgs> where TEventArgs : EventArgs {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="executionLevel">defines the execution level of the registrar</param>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="command">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public Composition(RegistrarExecutionStep executionLevel, Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute, string command, KeyValuePair<string, string> description) {
            UniqueId = Guid.NewGuid().ToString();

            ExecutionStep = executionLevel;
            InnerMethod = composition;
            CanExecute = canExecute ?? (obj => true);
            Command = command ?? string.Empty;
            Description = description;
        }

        #region MEMBERS

        public RegistrarExecutionStep ExecutionStep { get; }
        public Func<TEventArgs, Task> InnerMethod { get; }
        public Predicate<TEventArgs> CanExecute { get; }
        public string Command { get; }
        public KeyValuePair<string, string> Description { get; }

        public string UniqueId { get; }

        #endregion

        #region METHODS

        public async Task InvokeAsync(TEventArgs args) {
            if (!CanExecute(args)) {
                return;
            }

            await InnerMethod.Invoke(args);
        }

        #endregion
    }
}