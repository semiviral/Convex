#region

using System;

#endregion

namespace Convex.Core.Plugins.Compositions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CompositionDescription : Attribute
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
