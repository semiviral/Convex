#region

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Event;
using Serilog;

#endregion

namespace Convex.Core.Net
{
    public sealed class Connection
    {
        public void Dispose()
        {
            _Client?.Dispose();
            _NetworkStream?.Dispose();
            _Reader?.Dispose();
            _Writer?.Dispose();

            IsInitialized = false;
            IsConnected = false;
        }

        #region MEMBERS

        public IAddress Address { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool Executing { get; private set; }

        private TcpClient _Client;
        private NetworkStream _NetworkStream;
        private StreamReader _Reader;
        private StreamWriter _Writer;

        #endregion

        #region INIT

        public async Task<bool> Initialize(IAddress address)
        {
            Address = address;

            Connected += (source, args) =>
            {
                IsConnected = true;
                return Task.CompletedTask;
            };

            Disconnected += (source, args) =>
            {
                IsConnected = false;
                return Task.CompletedTask;
            };

            await AttemptConnect();

            IsInitialized = IsConnected;

            if (IsInitialized)
            {
                await OnInitialized(this, new ClassInitializedEventArgs(this));
            }

            return IsInitialized;
        }

        private async Task AttemptConnect()
        {
            try
            {
                await ConnectAsync();

                Log.Information($"Connection established to endpoint: {Address}");
                await OnConnected(this,
                    new ConnectedEventArgs(this, $"Connection established to endpoint: {Address} "));
            }
            catch (SocketException ex)
            {
                Log.Fatal($"Unable to connect to endpoint {Address}: {ex.Message}");
                await OnDisconnected(this,
                    new DisconnectedEventArgs(this, $"Unable to connect to endpoint {Address}."));
            }
            catch (Exception ex)
            {
                Log.Fatal($"Unable to connect to endpoint {Address}: {ex.Message}");
                await OnDisconnected(this,
                    new DisconnectedEventArgs(this, $"Unable to connect to endpoint {Address}."));
            }
        }

        #endregion

        #region METHODS

        public async Task SendDataAsync(object source, IrcCommandEventArgs args)
        {
            await WriteAsync(args.ToString());
        }

        /// <summary>
        ///     Receives input from open stream
        /// </summary>
        /// <remarks>
        ///     Do not use this method to start listening cycle.
        /// </remarks>
        public async Task<string> ListenAsync()
        {
            string data = string.Empty;
            Executing = true;

            try
            {
                data = await ReadAsync();
            }
            catch (NullReferenceException)
            {
                Log.Information("Stream disconnected. Attempting to reconnect...");

                await AttemptConnect();
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occured while listening on stream: {ex.Message}");
            }

            Executing = false;

            return data;
        }

        private async Task ConnectAsync()
        {
            _Client = new TcpClient();
            await _Client.ConnectAsync(Address.Hostname, Address.Port);

            _NetworkStream = _Client.GetStream();
            _Reader = new StreamReader(_NetworkStream);
            _Writer = new StreamWriter(_NetworkStream);
        }

        private async Task WriteAsync(string writable)
        {
            if (_Writer.BaseStream == null)
            {
                throw new NullReferenceException(nameof(_Writer.BaseStream));
            }

            await _Writer.WriteLineAsync(writable);
            await _Writer.FlushAsync();

            await OnFlushed(this, new StreamFlushedEventArgs(writable));
        }

        private async Task<string> ReadAsync()
        {
            if (_Reader.BaseStream == null)
            {
                throw new NullReferenceException(nameof(_Reader.BaseStream));
            }

            return await _Reader.ReadLineAsync();
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ClassInitializedEventArgs> Initialized;
        public event AsyncEventHandler<ConnectedEventArgs> Connected;
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        public event AsyncEventHandler<StreamFlushedEventArgs> Flushed;
        public event AsyncEventHandler<LogEventArgs> Logged;

        private async Task OnInitialized(object source, ClassInitializedEventArgs args)
        {
            if (Initialized == null)
            {
                return;
            }

            await Initialized.Invoke(source, args);
        }

        private async Task OnConnected(object source, ConnectedEventArgs args)
        {
            if (Connected == null)
            {
                return;
            }

            await Connected.Invoke(source, args);
        }

        private async Task OnDisconnected(object source, DisconnectedEventArgs args)
        {
            if (Disconnected == null)
            {
                return;
            }

            await Disconnected.Invoke(source, args);
        }

        private async Task OnFlushed(object source, StreamFlushedEventArgs args)
        {
            if (Flushed == null)
            {
                return;
            }

            await Flushed.Invoke(source, args);
        }

        #endregion
    }
}
