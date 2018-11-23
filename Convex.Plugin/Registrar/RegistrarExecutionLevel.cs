namespace Convex.Plugin.Registrar {
    public enum RegistrarExecutionLevel {
        /// <summary>
        /// <para>    This layer executes before all others.</para>
        /// <para>    This layer should only operate on the core
        ///             plugin layer.</para>
        ///         
        /// </summary>
        PreExecution = 0,

        /// <summary>
        ///     Determines whether the object should be executed by
        ///         lower execution levels.
        /// </summary>
        Critical = 1,

        /// <summary>
        ///     Layer for editing the object.
        /// </summary>
        Provisionary = 2,

        /// <summary>
        ///     Layer for external execution of plugin code.
        /// </summary>
        NonCritical = 3,
    }
}
