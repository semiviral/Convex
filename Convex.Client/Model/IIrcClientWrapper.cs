using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Net;
using Convex.Plugin.Registrar;

namespace Convex.Client.Model {
    public interface IIrcClientWrapper : IDisposable {
        #region MEMBERS

        bool IsInitialised { get; }

        ObservableCollection<Channel> Channels { get; }
        SortedList<Tuple<int, DateTime, Channel>, ServerMessage> Messages { get; }

        #endregion

        #region INIT

        Task Initialise(IAddress address);

        #endregion

        #region METHODS 

        void RegisterMethod(MethodRegistrar<ServerMessagedEventArgs> args);
        int GetMaxIndex();
        Channel GetChannel(string channelName);
        Task SendMessageAsync(object sender, IrcCommandEventArgs args);

        #endregion

        #region RUNTIME

        Task BeginListenAsync();

        #endregion
    }
}