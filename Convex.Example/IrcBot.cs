#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Convex.Core;
using Convex.Core.Net;
using Convex.Event;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Convex.Util;
using Serilog;
using Serilog.Events;

#endregion

namespace Convex.Example
{
    public class IrcBot : IDisposable
    {
        /// <summary>
        ///     Initialises class
        /// </summary>
        public IrcBot()
        {
            _bot = new IrcClient(FormatServerMessage, OnInvokedMethod);
            _bot.Initialised += (sender, args) =>
                OnLog(sender, new LogEventArgs(LogEventLevel.Information, "Client initialised."));
            _bot.Server.Connection.Flushed += (sender, args) =>
                OnLog(sender, new LogEventArgs(LogEventLevel.Information, $" >> {args.Information}"));
            _bot.Server.ServerMessaged += LogServerMessage;
        }

        #region INIT

        public async Task Initialise()
        {
            await _bot.Initialise(new Address("irc.foonetic.net", 6667));
            RegisterMethods();

            IsInitialised = true;
        }

        #endregion

        #region RUNTIME

        public async Task Execute()
        {
            await _bot.BeginListenAsync();
        }

        #endregion

        #region MEMBERS

        private string BotInfo =>
            $"[Version {_bot.Version}] Evealyn is an IRC bot for C#.";

        public bool IsInitialised { get; private set; }
        public bool Executing => _bot.Server.Executing;
        private readonly IIrcClient _bot;

        private readonly string[] _defaultChannels =
        {
            "#testgrounds"
        };

        #endregion

        #region EVENTS

        private static Task OnLog(object sender, LogEventArgs args)
        {
            switch (args.Level)
            {
                case LogEventLevel.Verbose:
                    Log.Verbose(args.Information);
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        private Task LogServerMessage(object source, ServerMessagedEventArgs e)
        {
            if (e.Message.Command.Equals(Commands.PRIVMSG))
            {
                OnLog(this,
                    new LogEventArgs(LogEventLevel.Information,
                        $"<{e.Message.Origin} {e.Message.Nickname}> {e.Message.Args}"));
            }
            else if (e.Message.Command.Equals(Commands.ERROR))
            {
                OnLog(this, new LogEventArgs(LogEventLevel.Error, e.Message.RawMessage));
            }
            else
            {
                OnLog(this, new LogEventArgs(LogEventLevel.Information, e.Message.RawMessage));
            }

            return Task.CompletedTask;
        }

        #endregion

        #region REGISTRARS

        /// <summary>
        ///     Register all methods
        /// </summary>
        private void RegisterMethods()
        {
            _bot.RegisterMethod(new Composition<ServerMessagedEventArgs>(99, Info,
                e => e.Message.InputCommand.Equals(nameof(Info).ToLower()),
                new CompositionDescription(nameof(Info), "returns the basic information about this bot"),
                Commands.PRIVMSG));
        }

        private async Task Info(ServerMessagedEventArgs e)
        {
            if ((e.Message.SplitArgs.Count < 2) || !e.Message.SplitArgs[1].Equals("info"))
            {
                return;
            }

            await _bot.Server.Connection.SendDataAsync(this,
                new IrcCommandEventArgs(Commands.PRIVMSG, $"{e.Message.Origin} {BotInfo}"));
        }

        #endregion

        #region DISPOSE

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bot?.Dispose();
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

        private static string FormatServerMessage(ServerMessage message)
        {
            return StaticLog.Format(message.Nickname, message.Args);
        }

        private async Task OnInvokedMethod(InvokedAsyncEventArgs<ServerMessagedEventArgs> args)
        {
            if (!args.Args.Message.Command.Equals(Commands.ALL))
            {
                await InvokeSteps(args, Commands.ALL);
            }

            if (!args.Host.CompositionHandlers.ContainsKey(args.Args.Message.Command) || !args.Args.Execute)
            {
                return;
            }

            await InvokeSteps(args, args.Args.Message.Command);
        }

        /// <summary>
        ///     Step-invokes an InvokedAsyncEventArgs
        /// </summary>
        /// <param name="args">InvokedAsyncEventArgs object</param>
        /// <param name="contextCommand">Command to execute from</param>
        /// <returns></returns>
        private async Task InvokeSteps(InvokedAsyncEventArgs<ServerMessagedEventArgs> args, string contextCommand)
        {
            foreach (IAsyncCompsition<ServerMessagedEventArgs> composition in args.Host
                .CompositionHandlers[contextCommand].OrderBy(comp => comp.ExecutionStep))
            {
                if (!args.Args.Execute)
                {
                    return;
                }

                await composition.InvokeAsync(args.Args);
            }
        }

        #endregion
    }
}
