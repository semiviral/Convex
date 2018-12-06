#region USINGS

using System;
using System.Threading.Tasks;
using Convex.Configuration;
using Convex.IRC;
using Serilog;

#endregion

namespace Convex.Example {
    public static class Program {
        #region MEMBERS

        private static IrcBot Bot { get; set; }

        #endregion

        private static async Task DebugRun() {
            do {
                await Bot.Execute();
            } while (Bot.Executing);

            Bot.Dispose();
        }

        private static async Task InitialiseAndExecute() {
            using (Bot = new IrcBot()) {
                await Bot.Initialise();
                await DebugRun();
            }
        }

        public static async Task Main() {
            Serilog.Log.Logger = new LoggerConfiguration().WriteTo.LiterateConsole().CreateLogger();
            Config.Initialise($@"{AppContext.BaseDirectory}\resource\");
            Serilog.Log.Logger = new LoggerConfiguration().WriteTo.LiterateConsole().WriteTo.RollingFile(Config.GetProperty("LogPath").ToString()).CreateLogger();

            await InitialiseAndExecute();

            Console.Write("Program terminated. Press any key to continue.");
            Console.ReadKey();
        }
    }
}