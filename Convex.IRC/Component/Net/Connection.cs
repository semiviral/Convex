#region usings

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Event;

#endregion

namespace Convex.IRC.Component.Net {
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

        public string Address { get; private set; }
        public int Port { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsInitialised { get; private set; }
        public bool Executing { get; set; }

        private TcpClient _client;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;

        #endregion

        #region INIT

        public async Task Initialise(string address, int port) {
            Address = address;
            Port = port;

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
                await Connect();

                await OnConnected(this, new ConnectedEventArgs(this, "Successfully connected to provided address."));
            } catch (Exception) {
                await OnDisconnected(this, new DisconnectedEventArgs(this, "Could not connect to provided address."));

                throw;
            }
        }

        #endregion

        #region METHODS

        public async Task SendDataAsync(object sender, IrcCommandRecievedEventArgs args) {
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
                await OnLogged(this, new InformationLoggedEventArgs("Stream disconnected. Attempting to reconnect..."));

                await AttemptConnect();
            } catch (Exception ex) {
                await OnLogged(this, new InformationLoggedEventArgs($"Exception occured while listening on stream: {ex.Message}"));
            }

            Executing = false;

            return data;
        }

        private async Task Connect() {
            _client = new TcpClient();
            await _client.ConnectAsync(Address, Port);

            _networkStream = _client.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream);
        }

        private async Task WriteAsync(string writable) {
            if (_writer.BaseStream == null)
                throw new NullReferenceException(nameof(_writer.BaseStream));

            await _writer.WriteLineAsync(writable);
            await _writer.FlushAsync();

            await OnFlushed(this, new StreamFlushedEventArgs(writable));
        }

        internal async Task<string> ReadAsync() {
            if (_reader.BaseStream == null)
                throw new NullReferenceException(nameof(_reader.BaseStream));

            return await _reader.ReadLineAsync();
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<ConnectedEventArgs> Connected;
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        public event AsyncEventHandler<StreamFlushedEventArgs> Flushed;
        public event AsyncEventHandler<InformationLoggedEventArgs> Logged;

        private async Task OnInitialised(object sender, ClassInitialisedEventArgs args) {
            if (Initialised == null)
                return;

            await Initialised.Invoke(sender, args);
        }

        private async Task OnConnected(object sender, ConnectedEventArgs args) {
            if (Connected == null)
                return;

            await Connected.Invoke(sender, args);
        }

        private async Task OnDisconnected(object sender, DisconnectedEventArgs args) {
            if (Disconnected == null)
                return;

            await Disconnected.Invoke(sender, args);
        }

        private async Task OnFlushed(object sender, StreamFlushedEventArgs args) {
            if (Flushed == null)
                return;

            await Flushed.Invoke(sender, args);
        }

        private async Task OnLogged(object sender, InformationLoggedEventArgs args) {
            if (Logged == null)
                return;

            await Logged.Invoke(sender, args);
        }

        #endregion
    }
}