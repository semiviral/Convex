#region

using System;

#endregion

namespace Convex.Core.Events
{
    public class ClassInitializedEventArgs : EventArgs
    {
        #region MEMBERS

        public object ClassObject { get; }

        #endregion

        public ClassInitializedEventArgs(object classObject)
        {
            ClassObject = classObject;
        }
    }
}