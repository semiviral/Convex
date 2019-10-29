#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Convex.Core.Component;
using Convex.Util;

#endregion

namespace Convex.Core.Net
{
    public class ServerMessage : Message
    {
        public ServerMessage(string rawData, Func<ServerMessage, string> formatter) : base(rawData)
        {
            if (rawData.StartsWith("ERROR"))
            {
                Command = Commands.ERROR;
                Args = rawData.Substring(rawData.IndexOf(' ') + 1);
                return;
            }

            Parse();

            _Formatter = formatter;
        }

        #region METHODS

        /// <summary>
        ///     For parsing IRCv3 message tags
        /// </summary>
        private void ParseTagsPrefix()
        {
            if (!RawMessage.StartsWith("@"))
            {
                return;
            }

            IsIrCv3Message = true;

            string fullTagsPrefix = RawMessage.Substring(0, RawMessage.IndexOf(' '));
            string[] primitiveTagsCollection = RawMessage.Split(';');

            foreach (string[] splitPrimitiveTag in primitiveTagsCollection.Select(primitiveTag =>
                primitiveTag.Split('=')))
            {
                Tags.Add(splitPrimitiveTag[0], splitPrimitiveTag[1] ?? string.Empty);
            }
        }

        public void Parse()
        {
            if (!_MessageRegex.IsMatch(RawMessage))
            {
                return;
            }

            ParseTagsPrefix();

            Timestamp = DateTime.Now;

            // begin parsing message into sections
            Match mVal = _MessageRegex.Match(RawMessage);
            Match sMatch = _SourceRegex.Match(mVal.Groups["source"].Value);

            // class property setting
            Nickname = mVal.Groups["source"].Value;
            RealName = mVal.Groups["source"].Value.ToLower();
            Hostname = mVal.Groups["source"].Value;
            Command = mVal.Groups["Type"].Value;
            Origin = mVal.Groups["Recipient"].Value.StartsWith(":")
                ? mVal.Groups["Recipient"].Value.Substring(1)
                : mVal.Groups["Recipient"].Value;

            Args = mVal.Groups["Args"].Value;

            // splits the first 5 sections of the message for parsing
            SplitArgs = Args.Split(new[]
            {
                ' '
            }, 4).Select(arg => arg.Trim()).ToList();

            if (!sMatch.Success)
            {
                return;
            }

            string realName = sMatch.Groups["Realname"].Value;
            Nickname = sMatch.Groups["Nickname"].Value;
            RealName = realName.StartsWith("~") ? realName.Substring(1) : realName;
            Hostname = sMatch.Groups["Hostname"].Value;
        }

        public override string ToString() => RawMessage;

        #endregion

        #region MEMBERS

        /// <summary>
        ///     Regex for parsing raw server message
        /// </summary>
        private static readonly Regex _MessageRegex =
            new Regex(@"^:(?<source>[^\s]+)\s(?<Type>[^\s]+)\s(?<Recipient>[^\s]+)\s?:?(?<Args>.*)",
                RegexOptions.Compiled);

        /// <summary>
        ///     Regex for parsing message source
        /// </summary>
        private static readonly Regex _SourceRegex =
            new Regex(@"^(?<Nickname>[^\s]+)!(?<Realname>[^\s]+)@(?<Hostname>[^\s]+)", RegexOptions.Compiled);

        private readonly Func<ServerMessage, string> _Formatter;

        public new string Formatted => _Formatter?.Invoke(this) ?? RawMessage;

        public bool IsIrCv3Message { get; private set; }

        public string RealName { get; set; }
        public string Hostname { get; set; }
        public string Command { get; set; }
        public string Args { get; set; }
        public List<string> SplitArgs { get; set; }

        public string InputCommand { get; set; } = string.Empty;

        public readonly Dictionary<string, string> Tags = new Dictionary<string, string>();

        #endregion
    }
}
