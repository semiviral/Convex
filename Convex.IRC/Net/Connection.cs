#region USINGS

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Event;
using Convex.IRC.Net.Event;
using Serilog.Events;

#endregion

namespace Convex.IRC.Net {
    public sealed class Connection : IConnection {
        public void Dispose() {
            _client?.Dispose();
            _networkStream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();

            IsInitialised = false;
            IsConnected = false;
        }

        #region MEMBERS

        public IAddress Address { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsInitialised { get; private set; }
        public bool Executing { get; set; }

        private TcpClient _client;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;

        #endregion

        #region INIT

        public async Task Initialise(IAddress address) {
            Address = address;

            Connected += (sender, args) => {
                IsConnected = true;
                return Task.CompletedTask;
            };

            Disconnected += (sender, args) => {
                IsConnected = false;
                return Task.CompletedTask;
            };

            await AttemptConnect();

            IsInitialised = IsConnected;
        }

        private async Task AttemptConnect() {
            try {
                await ConnectAsync();

                await OnConnected(this, new ConnectedEventArgs(this));
            } catch (Exception) {
                await OnDisconnected(this, new DisconnectedEventArgs(this));

                throw;
            }
        }

        #endregion

        #region METHODS

        public async Task SendDataAsync(object sender, IrcCommandEventArgs args) {
            await WriteAsync(args.ToString());
        }

        /// <summary>
        ///     Recieves input from open stream
        /// </summary>
        /// <remarks>
        ///     Do not use this method to start listening cycle.
        /// </remarks>
        public async Task<string> ListenAsync() {
            string data = string.Empty;
            Executing = true;

            try {
                data = await ReadAsync();
            } catch (NullReferenceException) {
                await OnLogged(this, new LogEventArgs(LogEventLevel.Information, "Stream disconnected. Attempting to reconnect..."));

                await AttemptConnect();
            } catch (Exception ex) {
                await OnLogged(this, new LogEventArgs(LogEventLevel.Information, $"Exception occured while listening on stream: {ex.Message}"));
            }

            Executing = false;

            return data;
        }

        private async Task ConnectAsync() {
            _client = new TcpClient();
            await _client.ConnectAsync(Address.Hostname, Address.Port);

            _networkStream = _client.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream);
        }

        private async Task WriteAsync(string writable) {
            if (_writer.BaseStream == null) {
                throw new NullReferenceException(nameof(_writer.BaseStream));
            }

            await _writer.WriteLineAsync(writable);
            await _writer.FlushAsync();

            await OnFlushed(this, new StreamFlushedEventArgs(writable));
        }

        internal async Task<string> ReadAsync() {
            if (_reader.BaseStream == null) {
                throw new NullReferenceException(nameof(_reader.BaseStream));
            }

            return await _reader.ReadLineAsync();
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<ConnectedEventArgs> Connected;
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        public event AsyncEventHandler<StreamFlushedEventArgs> Flushed;
        public event AsyncEventHandler<LogEventArgs> Logged;

        private async Task OnInitialised(object sender, ClassInitialisedEventArgs args) {
            if (Initialised == null) {
                return;
            }

            await Initialised.Invoke(sender, args);
        }

        private async Task OnConnected(object sender, ConnectedEventArgs args) {
            if (Connected == null) {
                return;
            }

            await Connected.Invoke(sender, args);
        }

        private async Task OnDisconnected(object sender, DisconnectedEventArgs args) {
            if (Disconnected == null) {
                return;
            }

            await Disconnected.Invoke(sender, args);
        }

        private async Task OnFlushed(object sender, StreamFlushedEventArgs args) {
            if (Flushed == null) {
                return;
            }

            await Flushed.Invoke(sender, args);
        }

        private async Task OnLogged(object sender, LogEventArgs args) {
            if (Logged == null) {
                return;
            }

            await Logged.Invoke(sender, args);
        }

        #endregion
    }
}