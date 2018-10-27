using System;
using Convex.Event;
using Convex.IRC.Component;
using Serilog;

namespace Convex.Client {
    public class StaticLog {
        public static void OnLog(object sender, LogEventArgs args) {
            switch (args.Level) {
                case Serilog.Events.LogEventLevel.Verbose:
                    Log.Verbose(args.Information);
                    break;
                case Serilog.Events.LogEventLevel.Debug:
                    Log.Debug(args.Information);
                    break;
                case Serilog.Events.LogEventLevel.Information:
                    Log.Information(args.Information);
                    break;
                case Serilog.Events.LogEventLevel.Warning:
                    Log.Warning(args.Information);
                    break;
                case Serilog.Events.LogEventLevel.Error:
                    Log.Error(args.Information);
                    break;
                case Serilog.Events.LogEventLevel.Fatal:
                    Log.Fatal(args.Information);
                    break;
            }
        }

        public static string FormatLogAsOutput(LogEventArgs args) {
            return $"[{nameof(args.Level).ToUpper()} {GetTime()}] {args.Information}\r\n";
        }

        public static string FormatLogAsOutput(ServerMessage message) {
            return $"[IRC {GetTime()}] <{message.Nickname}> {message.Args}";
        }

        public static string FormatLogAsOutput(string nickname, string message) {
            return $"[IRC {GetTime()}] <{nickname}> {message}";
        }

        public static string GetTime() {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
