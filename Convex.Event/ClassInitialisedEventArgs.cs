using System;

namespace Convex.Event {
    public class ClassInitialisedEventArgs : EventArgs {
        #region MEMBERS

        public object ClassObject { get; }

        #endregion

        public ClassInitialisedEventArgs(object classObject) {
            ClassObject = classObject;
        }
    }
}
