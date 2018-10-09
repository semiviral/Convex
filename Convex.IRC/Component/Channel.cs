#region usings

using System.Collections.Generic;
using Convex.IRC.Component.Reference;

#endregion

namespace Convex.IRC.Component {
    public class Channel {
        #region MEMBERS

        public string Name { get; }
        public string Topic { get; internal set; }
        public List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }
        public bool IsPrivate { get; }
        public bool Connected { get; internal set; }

        #endregion

        public Channel(string name) {
            Name = name;
            Topic = string.Empty;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
            IsPrivate = !Name.StartsWith("#");
        }
    }
}
