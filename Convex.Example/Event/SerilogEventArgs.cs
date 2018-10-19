using Convex.Event;
using Serilog.Events;

namespace Convex.Example.Event {
    internal class AdvancedLoggingEventArgs : InformationLoggedEventArgs {
        public AdvancedLoggingEventArgs(LogEventLevel logLevel, string information) : base(information) {
            Information = information;
            Level = logLevel;
        }

        #region MEMBERS

        public LogEventLevel Level { get; }

        #endregion
    }
}