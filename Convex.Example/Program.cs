#region

using System;
using System.Threading.Tasks;
using Convex.Core.Net;

#endregion

namespace Convex.Example
{
    public static class Program
    {
        #region MEMBERS

        private static IrcBot Bot { get; set; }

        #endregion

        private static async Task Run()
        {
            do
            {
                await Bot.Execute(new Address("irc.rizon.net", 6667));
            } while (Bot.Executing);

            Bot.Dispose();
        }

        private static async Task InitializeAndExecute()
        {
            using (Bot = new IrcBot())
            {
                await Bot.Initialize();

                if (!Bot.IsInitialised)
                {
                    return;
                }

                await Run();
            }
        }

        public static async Task Main()
        {
            await InitializeAndExecute();

            Bot.Dispose();

            Console.Write("Program terminated. Press any key to continue.");
            Console.ReadKey();
        }
    }
}