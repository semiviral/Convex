#region

using Convex.Core;
using Convex.Core.Net;
using Convex.Core.Plugins.Compositions;
using Serilog;
using System;
using System.Threading.Tasks;

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
        }

        #region INIT

        public async Task Initialize()
        {
            try
            {
                _BotInitialized = await _Bot.Initialize();

                _BotInitialized.Server.MessageReceived += (source, args) =>
                {
                    Log.Information(string.Format(_SERVER_MESSAGE_OUTPUT_FORMAT, args.Message.Nickname, args.Message.Args));
                    return Task.CompletedTask;
                };

                _BotInitialized.Server.Connection.Flushed += (sender, args) =>
                {
                    Log.Information($"   >> {args.Information}");
                    return Task.CompletedTask;
                };

                RegisterMethods();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsInitialised = _Bot?.Initialized ?? false;
            }
        }

        #endregion

        #region RUNTIME

        public async Task Execute(IAddress address)
        {
            await _BotInitialized.Connect(address);
            await _BotInitialized.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        private string BotInfo =>
            $"[Version {_Bot.AssemblyVersion}] Evealyn is an IRC bot for C#.";

        public bool IsInitialised { get; private set; }
        public bool Executing => _BotInitialized.Server.Connection.Connected;
        private readonly IClient _Bot;
        private  IInitializedClient _BotInitialized;

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

            await _BotInitialized.Server.Connection.SendCommandAsync(new CommandEventArgs(Commands.PRIVMSG, $"{args.Message.Origin} {BotInfo}"));
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