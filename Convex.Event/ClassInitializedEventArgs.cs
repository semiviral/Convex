#region

using System;

#endregion

namespace Convex.Event
{
    public class ClassInitializedEventArgs : EventArgs
    {
        public ClassInitializedEventArgs(object classObject) => ClassObject = classObject;

        #region MEMBERS

        public object ClassObject { get; }

        #endregion
    }
}
