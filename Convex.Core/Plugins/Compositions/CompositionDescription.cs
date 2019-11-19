#region

using System;

#endregion

namespace Convex.Core.Plugins.Compositions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CompositionDescription : Attribute
    {
        public string Command { get; }
        public string Description { get; }

        public CompositionDescription(string command, string description)
        {
            Command = command;
            Description = description;
        }

        public static explicit operator ValueTuple<string, string>(CompositionDescription compositionDescription)
        {
            return (compositionDescription.Command, compositionDescription.Description);
        }
    }
}