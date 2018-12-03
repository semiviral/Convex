#region usings

using System.Collections.Generic;
using Convex.Util;

#endregion

namespace Convex.Core.Component {
    public class Channel {
        public Channel(string name) {
            Name = name;
            Topic = string.Empty;
            Inhabitants = new List<string>();
            Modes = new List<IrcMode>();
            IsPrivate = !Name.StartsWith("#");
        }

        #region MEMBERS

        public string Name { get; }
        public string Topic { get; internal set; }
        public List<string> Inhabitants { get; }
        public List<IrcMode> Modes { get; }
        public bool IsPrivate { get; }
        public bool Connected { get; internal set; }

        #endregion
    }
}