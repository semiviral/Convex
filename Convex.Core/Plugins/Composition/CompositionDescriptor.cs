#region

using System;

#endregion

namespace Convex.Core.Plugins.Composition
{
    public class CompositionDescription
    {
        public CompositionDescription(string command, string description)
        {
            Command = command;
            Description = description;
        }

        public string Command { get; }
        public string Description { get; }

        public static explicit operator ValueTuple<string, string>(CompositionDescription compositionDescription)
            => (compositionDescription.Command, compositionDescription.Description);
    }
}
