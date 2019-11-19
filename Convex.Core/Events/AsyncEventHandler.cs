#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Core.Events
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs args) where TEventArgs : EventArgs;
}