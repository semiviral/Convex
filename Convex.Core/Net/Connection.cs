#region

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Core.Events;
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

            Connected = false;
        }

        #region MEMBERS

        public IAddress Address { get; }
        public bool Connected { get; private set; }
        public bool Executing { get; private set; }

        private TcpClient _Client;
        private NetworkStream _NetworkStream;
        private StreamReader _Reader;
        private StreamWriter _Writer;

        #endregion

        public Connection(IAddress address)
        {
            Address = address;

            Established += (source, args) =>
            {
                Connected = true;
                return Task.CompletedTask;
            };

            Disconnected += (source, args) =>
            {
                Connected = false;
                return Task.CompletedTask;
            };
        }

        public async Task Connect()
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

        #region METHODS

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

                await Connect();
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

            return await _Reader.ReadLineAsync() ?? string.Empty;
        }

        public async Task SendCommandAsync(CommandEventArgs args)
        {
            await WriteAsync(args.ToString());
        }

        #endregion

        #region EVENTS

        public event AsyncEventHandler<ConnectedEventArgs> Established;
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        public event AsyncEventHandler<StreamFlushedEventArgs> Flushed;

        private async Task OnConnected(object source, ConnectedEventArgs args)
        {
            if (Established == null)
            {
                return;
            }

            await Established.Invoke(source, args);
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
