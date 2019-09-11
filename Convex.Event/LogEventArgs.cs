#region

using System;
using Serilog.Events;

#endregion

namespace Convex.Event
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(LogEventLevel logLevel, string message)
        {
            Information = message;
            Level = logLevel;
        }

        #region MEMBERS

        public LogEventLevel Level { get; }
        public string Information { get; }

        #endregion
    }
}
