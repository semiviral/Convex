using System;
using Convex.Event;
using Convex.IRC.Component;
using Serilog;
using Serilog.Events;

namespace Convex.Client {
    public class StaticLog {
        public static void LogInformation(object sender, LogEventArgs args) {
            switch (args.Level) {
                case LogEventLevel.Verbose:
                    Log.Verbose(FormatLogAsOutput(args));
                    break;
                case LogEventLevel.Debug:
                    Log.Debug(args.Information);
                    break;
                case LogEventLevel.Information:
                    Log.Information(args.Information);
                    break;
                case LogEventLevel.Warning:
                    Log.Warning(args.Information);
                    break;
                case LogEventLevel.Error:
                    Log.Error(args.Information);
                    break;
                case LogEventLevel.Fatal:
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
