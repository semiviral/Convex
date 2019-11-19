#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convex.Core.Events;
using Serilog;

#endregion

namespace Convex.Core.Net
{
    public sealed class Connection : IConnection, IEstablishedConnection
    {
        private TcpClient _Client;
        private NetworkStream _NetworkStream;
        private StreamReader _Reader;
        private StreamWriter _Writer;

        public event AsyncEventHandler<ConnectedEventArgs> Established;
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;
        public event AsyncEventHandler<StreamDataEventArgs> DataWritten;
        public event AsyncEventHandler<StreamDataEventArgs> DataReceived;

        public Connection()
        {
            Established += (source, args) =>
            {
                _Connected = true;
                return Task.CompletedTask;
            };

            Disconnected += (source, args) =>
            {
                _Connected = false;
                return Task.CompletedTask;
            };
        }

        private async Task<string> ReadLineAsync()
        {
            if (_Reader.BaseStream == null)
            {
                throw new NullReferenceException(nameof(_Reader.BaseStream));
            }

            return await _Reader.ReadLineAsync() ?? string.Empty;
        }

        /// <summary>
        ///     Receives input from open stream
        /// </summary>
        /// <remarks>
        ///     Do not use this method to start listening cycle.
        /// </remarks>
        private async Task LongRunningListenAsync()
        {
            try
            {
                while (_Client.Connected)
                {
                    string data = await ReadLineAsync();

                    await OnDataReceived(this, new StreamDataEventArgs(data));
                }
            }
            catch (NullReferenceException)
            {
                Log.Information("Stream disconnected.");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occured while listening on stream: {ex.Message}");
            }
        }

        #region IConnection

        private bool _Connected;

        bool IConnection.Connected => _Connected;

        IEstablishedConnection IConnection.EstablishedConnection
        {
            get
            {
                if (!_Connected)
                {
                    throw new Exception("Connection is not yet established.");
                }

                return this;
            }
        }

        async Task<IEstablishedConnection> IConnection.Connect(IAddress address)
        {
            // todo logging here should be more informative

            IEstablishedConnection establishedConnection = null;

            try
            {
                _Address = address;

                _Client = new TcpClient();
                await _Client.ConnectAsync(_Address.Hostname, _Address.Port);

                _NetworkStream = _Client.GetStream();
                _Reader = new StreamReader(_NetworkStream);
                _Writer = new StreamWriter(_NetworkStream);
                _Connected = true;

                Log.Information($"Connection established to endpoint: {_Address}");
                await OnConnected(this,
                    new ConnectedEventArgs(this, $"Connection established to endpoint: {_Address} "));

                Log.Information("Starting long running connection listener task.");
                await Task.Factory.StartNew(LongRunningListenAsync, TaskCreationOptions.LongRunning);

                establishedConnection = this;
            }
            catch (SocketException ex)
            {
                Log.Fatal($"Unable to connect to endpoint {_Address}: {ex.Message}");
                await OnDisconnected(this,
                    new DisconnectedEventArgs(this, $"Unable to connect to endpoint {_Address}."));
            }
            catch (Exception ex)
            {
                Log.Fatal($"Unable to connect to endpoint {_Address}: {ex.Message}");
                await OnDisconnected(this,
                    new DisconnectedEventArgs(this, $"Unable to connect to endpoint {_Address}."));
            }

            if (establishedConnection == null)
            {
                throw new Exception("Unable to establish connection.");
            }

            return establishedConnection;
        }

        #endregion


        #region IEstablishedConnection

        private IAddress _Address;

        IAddress IEstablishedConnection.Address => _Address;
        
        async Task IEstablishedConnection.WriteAsync(string writable)
        {
            if (_Writer.BaseStream == null)
            {
                throw new NullReferenceException(nameof(_Writer.BaseStream));
            }

            await _Writer.WriteLineAsync(writable);
            await _Writer.FlushAsync();

            await OnDataWritten(this, new StreamDataEventArgs(writable));
        }

        async Task IEstablishedConnection.SendCommandAsync(CommandEventArgs args)
        {
            await ((IEstablishedConnection)this).WriteAsync(args.ToString());
        }


        private async Task OnConnected(object sender, ConnectedEventArgs args)
        {
            if (Established == null)
            {
                return;
            }

            await Established.Invoke(sender, args);
        }

        private async Task OnDisconnected(object sender, DisconnectedEventArgs args)
        {
            if (Disconnected == null)
            {
                return;
            }

            await Disconnected.Invoke(sender, args);
        }

        private async Task OnDataWritten(object sender, StreamDataEventArgs args)
        {
            if (DataWritten == null)
            {
                return;
            }

            await DataWritten.Invoke(sender, args);
        }

        private async Task OnDataReceived(object sender, StreamDataEventArgs args)
        {
            if (DataReceived == null)
            {
                return;
            }

            await DataReceived.Invoke(sender, args);
        }

        #endregion


        #region IDisposable

        private bool _Disposed;

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose)
            {
                return;
            }

            _Disposed = true;
            _Connected = false;

            _Client?.Dispose();
            _NetworkStream?.Dispose();
            _Reader?.Dispose();
            _Writer?.Dispose();

        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}