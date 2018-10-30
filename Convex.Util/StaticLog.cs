using System;
using Convex.Event;
using Serilog.Events;

namespace Convex.Util {
    public class StaticLog {
        public static void Log(LogEventArgs args) {
            switch (args.Level) {
                case LogEventLevel.Verbose:
                    Serilog.Log.Verbose(FormatLogAsOutput(args));
                    break;
                case LogEventLevel.Debug:
                    Serilog.Log.Debug(args.Information);
                    break;
                case LogEventLevel.Information:
                    Serilog.Log.Information(args.Information);
                    break;
                case LogEventLevel.Warning:
                    Serilog.Log.Warning(args.Information);
                    break;
                case LogEventLevel.Error:
                    Serilog.Log.Error(args.Information);
                    break;
                case LogEventLevel.Fatal:
                    Serilog.Log.Fatal(args.Information);
                    break;
            }
        }

        public static string FormatLogAsOutput(LogEventArgs args) {
            return $"[{nameof(args.Level).ToUpper()} {GetTime()}] {args.Information}\r\n";
        }

        public static string FormatLogAsOutput(string nickname, string message) {
            return $"[IRC {GetTime()}] <{nickname}> {message}";
        }

        public static string GetTime() {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
