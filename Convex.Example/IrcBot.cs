#region

using System;
using System.Threading.Tasks;
using Convex.Core;
using Convex.Core.Net;
using Convex.Core.Plugins.Composition;
using Serilog;

#endregion

namespace Convex.Example
{
    public class IrcBot : IDisposable
    {
        private const string _SERVER_MESSAGE_OUTPUT_FORMAT = "<{0}> {1}";

        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot()
        {
            _Bot = new Client();
            _Bot.Server.MessageReceived += (source, args) =>
            {
                Log.Information(string.Format(_SERVER_MESSAGE_OUTPUT_FORMAT, args.Message.Nickname, args.Message.Args));
                return Task.CompletedTask;
            };
            _Bot.Server.Connection.Flushed += (sender, args) =>
            {
                Log.Information($"   >> {args.Information}");
                return Task.CompletedTask;
            };
        }

        #region INIT

        public async Task Initialize()
        {
            try
            {
                if (!await _Bot.Initialize(new Address("irc.rizon.net", 6667)))
                {
                    return;
                }

                RegisterMethods();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsInitialised = _Bot?.IsInitialized ?? false;
            }
        }

        #endregion

        #region RUNTIME

        public async Task Execute()
        {
            await _Bot.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        private string BotInfo =>
            $"[Version {_Bot.Version}] Evealyn is an IRC bot for C#.";

        public bool IsInitialised { get; private set; }
        public bool Executing => _Bot.Server.Executing;
        private readonly IClient _Bot;

        private readonly string[] _DefaultChannels =
        {
            "#testgrounds"
        };

        #endregion

        #region REGISTRARS

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods()
        {
            _Bot.RegisterMethod(new MethodComposition<ServerMessagedEventArgs>(Info,
                new Composition(00, Commands.PRIVMSG),
                new CompositionDescription(nameof(Info), "returns the basic information about this bot")));
        }

        private async Task Info(ServerMessagedEventArgs args)
        {
            if (!args.Message.InputCommand.Equals(nameof(Info).ToLower()))
            {
                return;
            }

            await _Bot.Server.Connection.SendDataAsync(this,
                new IrcCommandEventArgs(Commands.PRIVMSG, $"{args.Message.Origin} {BotInfo}"));
        }

        #endregion

        #region DISPOSE

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Bot?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IrcBot()
        {
            Dispose(false);
        }

        #endregion

        #region METHODS

        private static string FormatServerMessage(ServerMessage message) => $"<{message.Nickname}> {message.Args}";

        #endregion
    }
}
