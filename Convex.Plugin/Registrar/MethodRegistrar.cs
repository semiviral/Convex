#region usings

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Registrar {
    public class Composition<TEventArgs> : IAsyncRegistrar<TEventArgs> where TEventArgs : EventArgs {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="executionLevel">defines the execution level of the registrar</param>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="command">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public Composition(RegistrarExecutionStep executionLevel, Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute, string command, Tuple<string, string> description) {
            IsRegistered = false;

            UniqueId = Guid.NewGuid().ToString();

            ExecutionStep = executionLevel;
            Composition = composition;
            CanExecute = canExecute ?? (obj => true);
            Command = command ?? string.Empty;
            Description = description;

            IsRegistered = true;
        }

        #region MEMBERS

        public RegistrarExecutionStep ExecutionStep { get; }
        public Func<TEventArgs, Task> Composition { get; }
        public Predicate<TEventArgs> CanExecute { get; }
        public string Command { get; }
        public Tuple<string, string> Description { get; }
        public bool IsRegistered { get; }

        public string UniqueId { get; }

        #endregion
    }
}