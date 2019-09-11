#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition
{
    public class Composition<TEventArgs> : IAsyncCompsition<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="executionLevel">defines the execution level of the registrar</param>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="command">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public Composition(
            int executionLevel, Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute,
            CompositionDescription description, params string[] commands)
        {
            UniqueId = Guid.NewGuid().ToString();

            ExecutionStep = executionLevel;
            InnerMethod = composition;
            CanExecute = canExecute ?? (obj => true);
            Commands = commands;
            Description = description ?? new CompositionDescription("", "undefined");
        }

        #region MEMBERS

        public int ExecutionStep { get; }
        public Func<TEventArgs, Task> InnerMethod { get; }
        public Predicate<TEventArgs> CanExecute { get; }
        public string[] Commands { get; }
        public CompositionDescription Description { get; }

        public string UniqueId { get; }

        #endregion

        #region METHODS

        public async Task InvokeAsync(TEventArgs args)
        {
            if (!CanExecute(args))
            {
                return;
            }

            await InnerMethod.Invoke(args);
        }

        #endregion
    }
}
