#region

using System;

#endregion

namespace Convex.Event
{
    public class ClassInitialisedEventArgs : EventArgs
    {
        public ClassInitialisedEventArgs(object classObject)
        {
            ClassObject = classObject;
        }

        #region MEMBERS

        public object ClassObject { get; }

        #endregion
    }
}
