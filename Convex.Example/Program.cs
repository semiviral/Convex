#region USINGS

using System;
using System.Threading.Tasks;

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
            await InitialiseAndExecute();

            Console.Write("Program terminated. Press any key to continue.");
            Console.ReadKey();
        }
    }
}