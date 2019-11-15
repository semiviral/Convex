#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Convex.Base.Calculator;
using Convex.Core;
using Convex.Core.Events;
using Convex.Core.Net;
using Convex.Core.Plugins;
using Convex.Core.Plugins.Compositions;
using SharpConfig;

#endregion

namespace Convex.Base
{
    public class Base : IPlugin
    {
        #region MEMBERS

        public string Name => "Base";
        public string Author => "Zavier Divelbiss";
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;
        public string Id => Guid.NewGuid().ToString();
        public PluginStatus Status { get; private set; } = PluginStatus.Stopped;

        private readonly InlineCalculator _Calculator = new InlineCalculator();

        private readonly Regex _YoutubeRegex =
            new Regex(@"(?i)http(?:s?)://(?:www\.)?youtu(?:be\.com/watch\?v=|\.be/)(?<ID>[\w\-]+)(&(amp;)?[\w\?=‌​]*)?",
                RegexOptions.Compiled);

        private bool MotdReplyEndSequenceEnacted { get; set; }
        private Configuration Configuration { get; set; }

        #endregion

        #region INTERFACE IMPLEMENTATION

        public event AsyncEventHandler<PluginActionEventArgs> Callback;

        public Task Start(Configuration configuration) => Task.CompletedTask;

        public async Task Stop()
        {
            if ((Status == PluginStatus.Running) || (Status == PluginStatus.Processing))
            {
                await Log($"Stop called but process is running from: {Name}");
            }
            else
            {
                await Log($"Plugin stopping: {Name}");
                await CallDie();
            }
        }

        public async Task CallDie()
        {
            Status = PluginStatus.Stopped;
            await Log($"Calling die, stopping process, sending unload —— plugin: {Name}");
        }

        #endregion

        #region METHODS

        private async Task Log(params string[] args)
        {
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.Log, string.Join(" ", args), Name));
        }

        private async Task DoCallback(object source, PluginActionEventArgs args)
        {
            if (Callback == null)
            {
                return;
            }

            args.PluginName = Name;

            await Callback.Invoke(source, args);
        }

        private static bool InputEquals(ServerMessagedEventArgs args, string comparison) =>
            args.Message.SplitArgs[0].Equals(comparison, StringComparison.OrdinalIgnoreCase);

        #endregion

        #region REGISTRARS

        [Composition(1, Commands.PRIVMSG)]
        private async Task Default(ServerMessagedEventArgs args)
        {
            if (Configuration[nameof(Core)]["IgnoreList"].StringValueArray.Contains(args.Message.RealName))
            {
                return;
            }

            if (!args.Message.SplitArgs[0].Replace(",", string.Empty)
                .Equals(Configuration[nameof(Core)]["Nickname"].StringValue.ToLower()))
            {
                return;
            }

            if (args.Message.SplitArgs.Count < 2)
            {
                // typed only 'eve'
                await DoCallback(this,
                    new PluginActionEventArgs(PluginActionType.SendMessage,
                        new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}",
                            "Type 'eve help' to view my command list."), Name));
                return;
            }

            // built-in 'help' command
            if (args.Message.SplitArgs[1].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Message.SplitArgs.Count.Equals(2))
                {
                    // in this case, 'help' is the only text in the string.
                    List<CompositionDescription> entries = args.Caller.PluginCommands.Values.ToList();
                    string commandsReadable = string.Join(", ",
                        entries.Where(entry => entry != null).Select(entry => entry.Command));

                    await DoCallback(this,
                        new PluginActionEventArgs(PluginActionType.SendMessage,
                            new CommandEventArgs(Commands.PRIVMSG,
                                entries.Count == 0
                                    ? $"{args.Message.Origin} No commands currently active."
                                    : $"{args.Message.Origin} Active commands: {commandsReadable}"), Name));
                    return;
                }

                CompositionDescription queriedCommand = args.Caller.GetDescription(args.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(null)
                    ? "Command not found."
                    : $"{queriedCommand.Command}: {queriedCommand.Description}";

                await DoCallback(this,
                    new PluginActionEventArgs(PluginActionType.SendMessage,
                        new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", valueToSend), Name));

                return;
            }

            if (args.Caller.CommandExists(args.Message.SplitArgs[1].ToLower()))
            {
                return;
            }

            await DoCallback(this,
                new PluginActionEventArgs(PluginActionType.SendMessage,
                    new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}",
                        "Invalid command. Type 'eve help' to view my command list."), Name));
        }

        [Composition(1, Commands.PRIVMSG)]
        private async Task MotdReplyEnd(ServerMessagedEventArgs args)
        {
            if (MotdReplyEndSequenceEnacted)
            {
                return;
            }

            await DoCallback(this,
                new PluginActionEventArgs(PluginActionType.SendMessage,
                    new CommandEventArgs(Commands.PRIVMSG,
                        $"NICKSERV IDENTIFY {Configuration[nameof(Core)]["Password"].StringValue}"),
                    Name));
            await DoCallback(this,
                new PluginActionEventArgs(PluginActionType.SendMessage,
                    new CommandEventArgs(Commands.MODE, $"{Configuration[nameof(Core)]["Nickname"].StringValue} +B"),
                    Name));

            //args.Caller.Server.Channels.Add(new Channel("#testgrounds"));

            MotdReplyEndSequenceEnacted = true;
        }

        [Composition(99, Commands.PRIVMSG)]
        [CompositionDescription(nameof(Quit), "terminates bot execution.")]
        private async Task Quit(ServerMessagedEventArgs args)
        {
            if (!InputEquals(args, "quit")
                || (args.Message.SplitArgs.Count < 2)
                || !args.Message.SplitArgs[1].Equals("quit"))
            {
                return;
            }

            await DoCallback(this,
                new PluginActionEventArgs(PluginActionType.SendMessage,
                    new CommandEventArgs(Commands.QUIT, "Shutting down."), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.Terminate, null, Name));
        }

        [Composition(99, Commands.PRIVMSG)]
        [CompositionDescription(nameof(Eval), "(<expression>) — evaluates given mathematical expression.")]
        private async Task Eval(ServerMessagedEventArgs args)
        {
            if (!InputEquals(args, "eval"))
            {
                return;
            }

            Status = PluginStatus.Processing;

            CommandEventArgs command =
                new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 3)
            {
                command.Arguments = "Not enough parameters.";
            }

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(command.Arguments))
            {
                Status = PluginStatus.Running;
                string evalArgs = args.Message.SplitArgs.Count > 3
                    ? args.Message.SplitArgs[2] + args.Message.SplitArgs[3]
                    : args.Message.SplitArgs[2];

                try
                {
                    command.Arguments = _Calculator.Evaluate(evalArgs).ToString(CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    command.Arguments = ex.Message;
                }
            }

            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));

            Status = PluginStatus.Stopped;
        }

        //private async Task Join(ServerMessagedEventArgs args) {
        //    Status = PluginStatus.Processing;

        //    string message = string.Empty;

        //    if (args.Message.SplitArgs.Count < 3) {
        //        message = "Insufficient parameters. Type 'eve help join' to view command's help index.";
        //    } else if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[2].StartsWith("#")) {
        //        message = "Channel name must start with '#'.";
        //    } else if (args.Caller.Server.GetChannel(args.Message.SplitArgs[2].ToLower()) != null) {
        //        message = "I'm already in that channel.";
        //    }

        //    Status = PluginStatus.Running;

        //    if (string.IsNullOrEmpty(message)) {
        //        await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.JOIN, args.Message.SplitArgs[2]), Name));

        //        args.Caller.Server.Channels.Add(new Channel(args.Message.SplitArgs[2].ToLower()));

        //        message = $"Successfully joined channel: {args.Message.SplitArgs[2]}.";
        //    }

        //    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", message), Name));

        //    Status = PluginStatus.Stopped;
        //}

        //private async Task Part(ServerMessagedEventArgs args) {
        //    Status = PluginStatus.Processing;

        //    IrcCommandEventArgs command = new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

        //    if (args.Message.SplitArgs.Count < 3) {
        //        command.Arguments = "Insufficient parameters. Type 'eve help part' to view command's help index.";
        //    } else if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[2].StartsWith("#")) {
        //        command.Arguments = "Channel parameter must be a proper name (starts with '#').";
        //    } else if (args.Message.SplitArgs.Count < 2 || args.Caller.Server.GetChannel(args.Message.SplitArgs[2]) == null) {
        //        command.Arguments = "I'm not in that channel.";
        //    }

        //    Status = PluginStatus.Running;

        //    if (!string.IsNullOrEmpty(command.Arguments)) {
        //        await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
        //        return;
        //    }

        //    string channel = args.Message.SplitArgs[2].ToLower();

        //    args.Caller.Server.RemoveChannel(channel);

        //    command.Arguments = $"Successfully parted channel: {channel}";

        //    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
        //    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.PART, $"{channel} Channel part invoked by: {args.Message.Nickname}"), Name));

        //    Status = PluginStatus.Stopped;
        //}

        //private async Task Channels(ServerMessagedEventArgs args) {
        //    Status = PluginStatus.Running;
        //    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Join(", ", args.Caller.Server.Channels.Where(channel => channel.Name.StartsWith("#")).Select(channel => channel.Name))), Name));

        //    Status = PluginStatus.Stopped;
        //}

        //private async Task YouTubeLinkResponse(ServerMessagedEventArgs args) {
        //    Status = PluginStatus.Running;

        //    const int maxDescriptionLength = 100;

        //    string getResponse = await $"https://www.googleapis.com/youtube/v3/videos?part=snippet&id={_youtubeRegex.Match(args.Message.Args).Groups["ID"]}&key={args.Caller.GetApiKey("YouTube")}".HttpGet();

        //    JToken video = JObject.Parse(getResponse)["items"][0]["snippet"];
        //    string channel = (string)video["channelTitle"];
        //    string title = (string)video["title"];
        //    string description = video["description"].ToString().Split('\n')[0];
        //    string[] descArray = description.Split(' ');

        //    if (description.Length > maxDescriptionLength) {
        //        description = string.Empty;

        //        for (int i = 0; description.Length < maxDescriptionLength; i++) {
        //            description += $" {descArray[i]}";
        //        }

        //        if (!description.EndsWith(" ")) {
        //            description.Remove(description.LastIndexOf(' '));
        //        }

        //        description += "....";
        //    }

        //    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", $"{title} (by {channel}) — {description}"), Name));

        //    Status = PluginStatus.Stopped;
        //}

        [Composition(99, Commands.PRIVMSG)]
        [CompositionDescription(nameof(Define), "(< word> *<part of speech>) — returns definition for given word.")]
        private async Task Define(ServerMessagedEventArgs args)
        {
            if (!InputEquals(args, "define"))
            {
                return;
            }

            Status = PluginStatus.Processing;

            CommandEventArgs command =
                new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 3)
            {
                command.Arguments = "Insufficient parameters. Type 'eve help define' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Running;

            string partOfSpeech = args.Message.SplitArgs.Count > 3
                ? $"&part_of_speech={args.Message.SplitArgs[3]}"
                : string.Empty;

            JsonDocument entry =
                JsonDocument.Parse(
                    await
                        $"http://api.pearson.com/v2/dictionaries/laad3/entries?headword={args.Message.SplitArgs[2]}{partOfSpeech}&limit=1"
                            .HttpGet());

            if (entry.RootElement.TryGetProperty("count", out JsonElement element)
                && (element.GetInt32() < 1))
            {
                command.Arguments = "Query returned no results.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Dictionary<string, string> _out = new Dictionary<string, string>
            {
                { "word", entry.RootElement.GetProperty("results")[0].GetProperty("headword").GetString() },
                { "pos", entry.RootElement.GetProperty("results")[0].GetProperty("part_of_speech").GetString() }
            };

            if (entry.RootElement.TryGetProperty("results", out JsonElement resultsElement)
                && resultsElement[0].TryGetProperty("senses", out JsonElement sensesElement))
            {
                if (sensesElement[0].TryGetProperty("subsenses", out JsonElement subSensesElement))
                {
                    // add definition
                    _out.Add("definition", subSensesElement[0].GetProperty("definition").GetString());

                    // check if example is provided and add
                    if (subSensesElement[0].TryGetProperty("examples", out JsonElement examplesElement))
                    {
                        _out.Add("example", examplesElement[0].GetProperty("text").GetString());
                    }
                }
                else
                {
                    _out.Add("definition", sensesElement[0].GetProperty("definition").GetString());

                    if (sensesElement[0].TryGetProperty("examples", out JsonElement examplesElement))
                    {
                        _out.Add("example", examplesElement[0].GetProperty("text").GetString());
                    }
                }
            }

            string returnMessage = $"{_out["word"]} [{_out["pos"]}] — {_out["definition"]}";

            if (_out.ContainsKey("example"))
            {
                returnMessage += $" (ex. {_out["example"]})";
            }

            command.Arguments = returnMessage;
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));

            Status = PluginStatus.Stopped;
        }

        [Composition(99, Commands.PRIVMSG)]
        [CompositionDescription(nameof(Lookup),
            "(<term/phrase>) — returns the wikipedia summary of given term or phrase.")]
        private async Task Lookup(ServerMessagedEventArgs args)
        {
            if (InputEquals(args, "lookup"))
            {
                return;
            }

            Status = PluginStatus.Processing;

            if ((args.Message.SplitArgs.Count < 2) || !args.Message.SplitArgs[1].Equals("lookup"))
            {
                return;
            }

            CommandEventArgs command =
                new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 3)
            {
                command.Arguments = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Running;

            string query = string.Join(" ", args.Message.SplitArgs.Skip(1));
            string response =
                await
                    $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}"
                        .HttpGet();

            JsonElement pages = JsonDocument.Parse(response).RootElement.GetProperty("query").GetProperty("pages")
                .EnumerateArray().First();

            if (!pages.TryGetProperty("extract", out JsonElement extractElement)
                || string.IsNullOrEmpty(extractElement.GetString()))
            {
                command.Arguments = "Query failed to return results. Perhaps try a different term?";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            string fullReplyStr =
                $"\x02{pages.GetProperty("title").GetString()}\x0F — {Regex.Replace(extractElement.GetString(), @"\n\n?|\n", " ")}";

            command.Command = args.Message.Nickname;

            foreach (string splitMessage in fullReplyStr.LengthSplit(400))
            {
                command.Arguments = splitMessage;
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
            }

            Status = PluginStatus.Stopped;
        }

        private async Task Set(ServerMessagedEventArgs args)
        {
            if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[1].Equals("set"))
            {
                return;
            }

            Status = PluginStatus.Processing;

            CommandEventArgs command =
                new CommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 5)
            {
                command.Arguments = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Stopped;
        }
    }

    #endregion
}
