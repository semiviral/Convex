using System;
using Convex.Event;
using Convex.IRC.Component;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace Convex.Client {
    public static class Program {
        public static void Main(string[] args) {
            Config = JsonConvert.DeserializeObject<Configuration>(DEFAULT_CONFIG);

            Log.Logger = new LoggerConfiguration().WriteTo.RollingFile(Config.LogFilePath).CreateLogger();

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
        }

        public static string FormatLogAsOutput(LogEventArgs args) {
            return $"[{nameof(args.Level).ToUpper()} {DateTime.Now}] {args.Information}\r\n";
        }

        public static string FormatLogAsOutput(ServerMessage message) {
            return $"[IRC {DateTime.Now}] <{message.Nickname}> {message.Args}";
        }

        public static string FormatLogAsOutput(string nickname, string message) {
            return $"[IRC {DateTime.Now}] <{nickname}> {message}";
        }

        #region MEMBERS

        public const string DEFAULT_CONFIG = "{\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": { \"YouTube\": \"\", \"Dictionary\": \"\" },\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseFilePath\": \"\",\r\n\t\"LogFilePath\": \"\"\r\n}\r\n";

        public static Configuration Config { get; private set; }

        #endregion
    }
}