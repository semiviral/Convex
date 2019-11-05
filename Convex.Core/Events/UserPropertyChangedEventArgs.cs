#region

using System.ComponentModel;

#endregion

namespace Convex.Core.Events
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows the notifier class to easily tag which property is changed by name, and its new value.
    /// </summary>
    public sealed class UserPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public UserPropertyChangedEventArgs(string propertyName, string name, object newValue) : base(propertyName)
        {
            Name = name;
            NewValue = newValue;
        }

        #region MEMBERS

        public string Name { get; }
        public object NewValue { get; }

        #endregion
    }
}
