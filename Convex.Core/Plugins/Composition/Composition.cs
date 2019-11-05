#region

using System;

#endregion

namespace Convex.Core.Plugins.Composition
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Composition : Attribute
    {
        public Composition(int priority, params string[] commands)
        {
            UniqueId = Guid.NewGuid().ToString();
            Priority = priority;
            Commands = commands;
        }

        #region MEMBERS

        public string UniqueId { get; }
        public int Priority { get; }
        public string[] Commands { get; }

        #endregion
    }
}
