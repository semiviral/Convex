#region

using System;
using System.Threading.Tasks;
using Convex.Core.Configuration;
using Serilog;

#endregion

namespace Convex.Example
{
    public static class Program
    {
        #region MEMBERS

        private static IrcBot Bot { get; set; }

        #endregion

        private static async Task DebugRun()
        {
            do
            {
                await Bot.Execute();
            } while (Bot.Executing);

            Bot.Dispose();
        }

        private static async Task InitializeAndExecute()
        {
            using (Bot = new IrcBot())
            {
                await Bot.Initialize();
                await DebugRun();
            }
        }

        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.LiterateConsole().CreateLogger();
            Config.Initialize($@"{AppContext.BaseDirectory}\resource\");
            Log.Logger = new LoggerConfiguration().WriteTo.LiterateConsole().WriteTo
                .RollingFile(Config.GetProperty("LogPath").ToString()).CreateLogger();

            await InitializeAndExecute();

            Console.Write("Program terminated. Press any key to continue.");
            Console.ReadKey();
        }
    }
}
