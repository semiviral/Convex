namespace Convex.Plugin.Registrar {
    /// <summary>
    ///     A registrar's execution step defines where in the chain of execution it falls.
    ///     0 is executed first, 1 second, etc.
    ///     
    ///     Step 0 should be mostly reserved for execution-halting initial checks.
    /// </summary>
    public enum RegistrarExecutionStep {
        Step0 = 0,
        Step1 = 1,
        Step2 = 2,
        Step3 = 3,
        Step4 = 4,
        Step5 = 5,
        Step6 = 6,
        Step7 = 7,
    }
}
