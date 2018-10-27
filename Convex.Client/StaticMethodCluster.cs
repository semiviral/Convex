using System;
using Convex.Event;
using Convex.IRC.Component;

namespace Convex.Client {
    public class LogStringFormatter {
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
