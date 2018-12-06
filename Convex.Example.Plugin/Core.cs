#region USINGS

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Convex.Configuration;
using Convex.Event;
using Convex.Example.Plugin.Calculator;
using Convex.Net;
using Convex.Plugin;
using Convex.Plugin.Composition;
using Convex.Plugin.Event;
using Convex.Util;
using Newtonsoft.Json.Linq;

#endregion

namespace Convex.Example.Plugin {
    public class Core : IPlugin {
        #region MEMBERS

        public string Name => "Core";
        public string Author => "Antonio DiNostri";
        public Version Version => new AssemblyName(GetType().GetTypeInfo().Assembly.FullName).Version;
        public string Id => Guid.NewGuid().ToString();
        public PluginStatus Status { get; private set; } = PluginStatus.Stopped;

        private readonly InlineCalculator _calculator = new InlineCalculator();
        private readonly Regex _youtubeRegex = new Regex(@"(?i)http(?:s?)://(?:www\.)?youtu(?:be\.com/watch\?v=|\.be/)(?<ID>[\w\-]+)(&(amp;)?[\w\?=‌​]*)?", RegexOptions.Compiled);

        private bool MotdReplyEndSequenceEnacted;

        #endregion

        #region INTERFACE IMPLEMENTATION

        public event AsyncEventHandler<PluginActionEventArgs> Callback;

        public async Task Start() {
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(1, Default, null, null, Commands.PRIVMSG), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, MotdReplyEnd, null, null, Commands.RPL_ENDOFMOTD), Name));
            //await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, YouTubeLinkResponse, args => _youtubeRegex.IsMatch(args.Message.Args), null, Commands.PRIVMSG), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Quit, args => InputEquals(args, "quit"), new CompositionDescription(nameof(Quit), "terminates bot execution"), Commands.PRIVMSG), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Eval, args => InputEquals(args, "eval"), new CompositionDescription(nameof(Eval), "(<expression>) — evaluates given mathematical expression."), Commands.PRIVMSG), Name));
            //await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Join, args => InputEquals(args, "join"), new CompositionDescription(nameof(Join), "(< channel> *<message>) — joins specified channel."), Commands.PRIVMSG), Name));
            //await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Part, args => InputEquals(args, "part"), new CompositionDescription(nameof(Part), "(< channel> *<message>) — parts from specified channel."), Commands.PRIVMSG), Name));
            //await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Channels, args => InputEquals(args, "channels"), new CompositionDescription(nameof(Channels), "returns a list of connected channels."), Commands.PRIVMSG), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Define, args => InputEquals(args, "define"), new CompositionDescription(nameof(Define), "(< word> *<part of speech>) — returns definition for given word."), Commands.PRIVMSG), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.RegisterMethod, new Composition<ServerMessagedEventArgs>(99, Lookup, args => InputEquals(args, "lookup"), new CompositionDescription(nameof(Lookup), "(<term/phrase>) — returns the wikipedia summary of given term or phrase."), Commands.PRIVMSG), Name));
        }

        public async Task Stop() {
            if (Status.Equals(PluginStatus.Running) || Status.Equals(PluginStatus.Processing)) {
                await Log($"Stop called but process is running from: {Name}");
            } else {
                await Log($"Plugin stopping: {Name}");
                await CallDie();
            }
        }

        public async Task CallDie() {
            Status = PluginStatus.Stopped;
            await Log($"Calling die, stopping process, sending unload —— plugin: {Name}");
        }

        #endregion

        #region METHODS

        private async Task Log(params string[] args) {
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.Log, string.Join(" ", args), Name));
        }

        private async Task DoCallback(object source, PluginActionEventArgs args) {
            if (Callback == null) {
                return;
            }

            args.PluginName = Name;

            await Callback.Invoke(source, args);
        }

        private bool InputEquals(ServerMessagedEventArgs args, string comparison) {
            return args.Message.SplitArgs[0].Equals(comparison, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region REGISTRARS

        private async Task Default(ServerMessagedEventArgs e) {
            if (((List<string>)Config.GetProperty("IgnoreList"))?.Contains(e.Message.Realname) == true) {
                return;
            }

            if (!e.Message.SplitArgs[0].Replace(",", string.Empty).Equals(Config.GetProperty("Nickname").ToString().ToLower())) {
                return;
            }

            if (e.Message.SplitArgs.Count < 2) {
                // typed only 'eve'
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {e.Message.Origin}", "Type 'eve help' to view my command list."), Name));
                return;
            }

            // built-in 'help' command
            if (e.Message.SplitArgs[1].Equals("help", StringComparison.OrdinalIgnoreCase)) {
                if (e.Message.SplitArgs.Count.Equals(2)) {
                    // in this case, 'help' is the only text in the string.
                    List<CompositionDescription> entries = e.Caller.LoadedDescriptions.Values.ToList();
                    string commandsReadable = string.Join(", ", entries.Where(entry => entry != null).Select(entry => entry.Command));

                    await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.PRIVMSG, entries.Count == 0 ? $"{e.Message.Origin} No commands currently active." : $"{e.Message.Origin} Active commands: {commandsReadable}"), Name));
                    return;
                }

                CompositionDescription queriedCommand = e.Caller.GetDescription(e.Message.SplitArgs[2]);

                string valueToSend = queriedCommand.Equals(null) ? "Command not found." : $"{queriedCommand.Command}: {queriedCommand.Description}";

                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {e.Message.Origin}", valueToSend), Name));

                return;
            }

            if (e.Caller.CommandExists(e.Message.SplitArgs[1].ToLower())) {
                return;
            }

            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs($"{Commands.PRIVMSG} {e.Message.Origin}", "Invalid command. Type 'eve help' to view my command list."), Name));
        }

        private async Task MotdReplyEnd(ServerMessagedEventArgs args) {
            if (MotdReplyEndSequenceEnacted) {
                return;
            }

            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.PRIVMSG, $"NICKSERV IDENTIFY {Config.GetProperty("Password")}"), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.MODE, $"{Config.GetProperty("nickname")} +B"), Name));

            //args.Caller.Server.Channels.Add(new Channel("#testgrounds"));

            MotdReplyEndSequenceEnacted = true;
        }

        private async Task Quit(ServerMessagedEventArgs args) {
            if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[1].Equals("quit")) {
                return;
            }

            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, new IrcCommandEventArgs(Commands.QUIT, "Shutting down."), Name));
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SignalTerminate, null, Name));
        }

        private async Task Eval(ServerMessagedEventArgs e) {
            Status = PluginStatus.Processing;

            IrcCommandEventArgs command = new IrcCommandEventArgs($"{Commands.PRIVMSG} {e.Message.Origin}", string.Empty);

            if (e.Message.SplitArgs.Count < 3) {
                command.Arguments = "Not enough parameters.";
            }

            Status = PluginStatus.Running;

            if (string.IsNullOrEmpty(command.Arguments)) {
                Status = PluginStatus.Running;
                string evalArgs = e.Message.SplitArgs.Count > 3 ? e.Message.SplitArgs[2] + e.Message.SplitArgs[3] : e.Message.SplitArgs[2];

                try {
                    command.Arguments = _calculator.Evaluate(evalArgs).ToString(CultureInfo.CurrentCulture);
                } catch (Exception ex) {
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

        private async Task Define(ServerMessagedEventArgs args) {
            Status = PluginStatus.Processing;

            IrcCommandEventArgs command = new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 3) {
                command.Arguments = "Insufficient parameters. Type 'eve help define' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Running;

            string partOfSpeech = args.Message.SplitArgs.Count > 3 ? $"&part_of_speech={args.Message.SplitArgs[3]}" : string.Empty;

            JObject entry = JObject.Parse(await $"http://api.pearson.com/v2/dictionaries/laad3/entries?headword={args.Message.SplitArgs[2]}{partOfSpeech}&limit=1".HttpGet());

            if ((int)entry.SelectToken("count") < 1) {
                command.Arguments = "Query returned no results.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Dictionary<string, string> _out = new Dictionary<string, string> { { "word", (string)entry["results"][0]["headword"] }, { "pos", (string)entry["results"][0]["part_of_speech"] } };

            // todo optimise if block
            // this 'if' block seems messy and unoptimised.
            // I'll likely change it in the future.
            if (entry["results"][0]["senses"][0]["subsenses"] != null) {
                _out.Add("definition", (string)entry["results"][0]["senses"][0]["subsenses"][0]["definition"]);

                if (entry["results"][0]["senses"][0]["subsenses"][0]["examples"] != null) {
                    _out.Add("example", (string)entry["results"][0]["senses"][0]["subsenses"][0]["examples"][0]["text"]);
                }
            } else {
                _out.Add("definition", (string)entry["results"][0]["senses"][0]["definition"]);

                if (entry["results"][0]["senses"][0]["examples"] != null) {
                    _out.Add("example", (string)entry["results"][0]["senses"][0]["examples"][0]["text"]);
                }
            }

            string returnMessage = $"{_out["word"]} [{_out["pos"]}] — {_out["definition"]}";

            if (_out.ContainsKey("example")) {
                returnMessage += $" (ex. {_out["example"]})";
            }

            command.Arguments = returnMessage;
            await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));

            Status = PluginStatus.Stopped;
        }

        private async Task Lookup(ServerMessagedEventArgs args) {
            Status = PluginStatus.Processing;

            if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[1].Equals("lookup")) {
                return;
            }

            IrcCommandEventArgs command = new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 3) {
                command.Arguments = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Running;

            string query = string.Join(" ", args.Message.SplitArgs.Skip(1));
            string response = await $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}".HttpGet();

            JToken pages = JObject.Parse(response)["query"]["pages"].Values().First();

            if (string.IsNullOrEmpty((string)pages["extract"])) {
                command.Arguments = "Query failed to return results. Perhaps try a different term?";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            string fullReplyStr = $"\x02{(string)pages["title"]}\x0F — {Regex.Replace((string)pages["extract"], @"\n\n?|\n", " ")}";

            command.Command = args.Message.Nickname;

            foreach (string splitMessage in fullReplyStr.LengthSplit(400)) {
                command.Arguments = splitMessage;
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
            }

            Status = PluginStatus.Stopped;
        }

        private async Task Set(ServerMessagedEventArgs args) {
            if (args.Message.SplitArgs.Count < 2 || !args.Message.SplitArgs[1].Equals("set")) {
                return;
            }

            Status = PluginStatus.Processing;

            IrcCommandEventArgs command = new IrcCommandEventArgs($"{Commands.PRIVMSG} {args.Message.Origin}", string.Empty);

            if (args.Message.SplitArgs.Count < 5) {
                command.Arguments = "Insufficient parameters. Type 'eve help lookup' to view correct usage.";
                await DoCallback(this, new PluginActionEventArgs(PluginActionType.SendMessage, command, Name));
                return;
            }

            Status = PluginStatus.Stopped;
        }
    }

    #endregion
}