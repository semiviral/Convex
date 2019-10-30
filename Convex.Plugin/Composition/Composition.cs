#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Plugin.Composition
{
    public class Composition<TEventArgs> : IAsyncComposition<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of MethodRegistrar
        /// </summary>
        /// <param name="priority">defines the execution priority of the registrar</param>
        /// <param name="canExecute">defines execution readiness</param>
        /// <param name="commands">command to reference composition</param>
        /// <param name="composition">registrable composition to be executed</param>
        /// <param name="description">describes composition</param>
        public Composition(int priority, Func<TEventArgs, Task> composition, Predicate<TEventArgs> canExecute,
            CompositionDescription description, params string[] commands)
        {
            UniqueId = Guid.NewGuid().ToString();

            Priority = priority;
            Method = composition;
            CanExecute = canExecute ?? (obj => true);
            Commands = commands;
            Description = description ?? new CompositionDescription("", "undefined");
        }

        #region MEMBERS

        public int Priority { get; }
        public string[] Commands { get; }
        public Func<TEventArgs, Task> Method { get; }
        public Predicate<TEventArgs> CanExecute { get; }
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

            await Method.Invoke(args);
        }

        #endregion
    }
}
