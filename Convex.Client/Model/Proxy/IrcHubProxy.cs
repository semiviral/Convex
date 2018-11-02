using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Model;
using Convex.Client.Model.Collections;
using Convex.Client.Models.Proxy;
using Convex.Client.Services;
using Convex.Event;
using Convex.IRC.Net;
using Convex.Util;

namespace Convex.Client.Proxy {
    public class IrcHubProxy : IIrcHubProxy {
        public IrcHubProxy(IIrcService ircService, IIrcHubMethodsProxy ircHubMethodsProxy) {
            _ircService = ircService;
            _ircService.IrcClientWrapper.Channels.CollectionChanged += OnChannelsChanged;

            _ircHubMethodsProxy = ircHubMethodsProxy;
            _previouslySentInputs = new SortedList<int, string>();
            _currentSentIndex = 0;
            _hasFirstElement = false;
        }

        #region MEMBERS

        private IIrcService _ircService;
        private IIrcHubMethodsProxy _ircHubMethodsProxy;
        private SortedList<int, string> _previouslySentInputs;
        private int _currentSentIndex;
        private bool _hasFirstElement;

        #endregion

        #region INTERFACE IMPLEMENTATION

        public Task StartAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        #endregion

        #region EVENTS

        private void OnChannelsChanged(object source, NotifyCollectionChangedEventArgs args) {
            foreach (Channel channel in args.NewItems) {
                switch (args.Action) {
                    case NotifyCollectionChangedAction.Add:
                        _ircHubMethodsProxy.AddChannel(channel.Name);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _ircHubMethodsProxy.RemoveChannel(channel.Name);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
            }
        }

        #endregion

        #region CLIENT TO SERVER METHODS

        public async Task SendMessage(string rawMessage) {
            if (string.IsNullOrWhiteSpace(rawMessage)) {
                return;
            }

            int currentMaxIndex = GetMaxPreviouslySentIndex();

            if (currentMaxIndex == 0 && !_hasFirstElement) {
                _previouslySentInputs.Add(currentMaxIndex, rawMessage);
                _hasFirstElement = true;
            } else {
                currentMaxIndex += 1;

                _previouslySentInputs.Add(currentMaxIndex, rawMessage);
            }

            _currentSentIndex = currentMaxIndex + 1;

            await _ircHubMethodsProxy.BroadcastMessage(StaticLog.Format(Program.Config.Nickname, rawMessage));
            await _ircService.IrcClientWrapper.SendMessageAsync(this, ConvertToCommandArgs(rawMessage));
        }

        #endregion

        #region SERVER TO CLIENT METHODS

        /// <summary>
        ///     Broadcasts a batch of messages.
        /// </summary>
        /// <param name="connectionId">Connection ID of client.</param>
        /// <param name="startDate">Start index. Cannot be negative.</param>
        /// <param name="endDate">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, string channelName, DateTime startDate, DateTime endDate) {
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startDate >= endDate || endDate <= DateTime.Now) {
                return;
            }
            await BroadcastMessageBatch(connectionId, isPrepend, GetMessagesFromChannel(channelName).Where(kvp => IsDateBetween(kvp.Key.Timestamp, startDate, endDate)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, string channelName, int startIndex, int length) {
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= length || length <= 0) {
                return;
            }

            await BroadcastMessageBatch(connectionId, isPrepend, GetMessagesFromChannel(channelName).Select(message => message.Value).Skip(startIndex).Take(length));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<ServerMessage> messageBatch) {
            if (isPrepend) {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(FormatServerMessage), true);
            } else {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(FormatServerMessage), false);
            }
        }


        public async Task UpdateMessageInput(string connectionId, bool previousMessage) {
            string updatedMessage = string.Empty;

            if (previousMessage) {
                if (_currentSentIndex - 1 < 0) {
                    return;
                }

                _currentSentIndex -= 1;

                updatedMessage = _previouslySentInputs[_currentSentIndex];
            } else {
                if (_currentSentIndex + 1 > GetMaxPreviouslySentIndex()) {
                    return;
                }

                _currentSentIndex += 1;

                updatedMessage = _previouslySentInputs[_currentSentIndex];
            }

            await _ircHubMethodsProxy.UpdateMessageInput(connectionId, updatedMessage);
        }


        #endregion

        #region METHODS

        private IEnumerable<KeyValuePair<MessagesIndex, ServerMessage>> GetMessagesFromChannel(string channelName) {
            return _ircService.IrcClientWrapper.Messages.Where(message => message.Key.ChannelName.Equals(channelName));
        }

        private int GetMaxPreviouslySentIndex() {
            return _previouslySentInputs.Keys.Max(key => key as int?) ?? 0;
        }

        private bool IsDateBetween(DateTime date, DateTime startDate, DateTime endDate) {
            return date >= startDate && date <= endDate;
        }

        private bool IsIntBetween(int value, int int1, int int2) {
            return value >= int1 && value <= int2;
        }

        private string FormatServerMessage(ServerMessage message) {
            return StaticLog.Format(message.Nickname, message.Args);
        }

        private IrcCommandEventArgs ConvertToCommandArgs(string rawMessage) {
            int firstSpaceIndex = rawMessage.IndexOf(' ') == -1 ? 0 : rawMessage.IndexOf(' ');
            string command = rawMessage.Substring(0, firstSpaceIndex).ToUpper();
            string content = rawMessage.Remove(0, firstSpaceIndex + 1);

            if (rawMessage.StartsWith('/')) {
                return new IrcCommandEventArgs(command.Remove(0, 1), content);
            } else {
                return new IrcCommandEventArgs(Commands.PRIVMSG, "#testgrounds " + rawMessage);
            }
        }

        #endregion
    }
}
