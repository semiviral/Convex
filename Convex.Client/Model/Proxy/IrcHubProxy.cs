using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convex.Client.Model;
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
            _selectedChannelName = string.Empty;
        }

        #region MEMBERS

        private IIrcService _ircService;
        private IIrcHubMethodsProxy _ircHubMethodsProxy;
        private SortedList<int, string> _previouslySentInputs;
        private int _currentSentIndex;
        private bool _hasFirstElement;
        private string _selectedChannelName;

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

        public Task UpdateSelectedChannel(string channelName) {
            _selectedChannelName = channelName;

            return Task.CompletedTask;
        }

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
        /// <param name="startIndex">Start index. Cannot be negative.</param>
        /// <param name="endIndex">Start index. Cannot be negative.</param>
        /// <param name="isPrepended">Defines if the batch needs to be sent as a prepend list.</param>
        /// <returns></returns>
        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, DateTime startIndex, DateTime endIndex) {
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= DateTime.Now) {
                return;
            }
            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.IrcClientWrapper.Messages.Where(kvp => IsDateBetween(kvp.Key.Item2, startIndex, endIndex)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, int startIndex, int endIndex) {
            if (_ircService.IrcClientWrapper.Messages.Count <= 0) {
                return;
            }

            if (startIndex >= endIndex || endIndex <= 0) {
                return;
            }

            await BroadcastMessageBatch(connectionId, isPrepend, _ircService.IrcClientWrapper.Messages.Where(kvp => IsIntBetween(kvp.Key.Item1, startIndex, endIndex)).Select(kvp => kvp.Value));
        }

        public async Task BroadcastMessageBatch(string connectionId, bool isPrepend, IEnumerable<ServerMessage> messageBatch) {
            if (isPrepend) {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(message => FormatServerMessage(message)), true);
            } else {
                await _ircHubMethodsProxy.BroadcastMessageBatch(connectionId, messageBatch.Select(message => FormatServerMessage(message)), false);
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

        private int GetMaxPreviouslySentIndex() {
            return _previouslySentInputs.Keys.Max(key => key as int?) ?? 0;
        }

        private IEnumerable<DateTime> GetDatesBetween(IEnumerable<DateTime> dateList, DateTime startDate, DateTime endDate) {
            foreach (DateTime date in dateList) {
                if (IsDateBetween(date, startDate, endDate)) {
                    yield return date;
                }
            }
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
