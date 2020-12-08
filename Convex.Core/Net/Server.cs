#region

using System;
using System.Threading.Tasks;

#endregion

namespace Convex.Core.Net
{
    public class Server : IDisposable
    {
        public IConnection Connection { get; }
        public bool Identified { get; set; }

        public Server()
        {
            Connection = new Connection();
        }


        /// <summary>
        ///     sends client info to the server
        /// </summary>
        public async Task SendIdentityInfo(string nickname, string realname)
        {
            await Connection.EstablishedConnection.SendCommandAsync(new CommandEventArgs(Commands.USER,
                $"{nickname} 0 * {realname}"));
            await Connection.EstablishedConnection.SendCommandAsync(new CommandEventArgs(Commands.NICK, nickname));

            Identified = true;
        }

        #region IDisposable

        private bool _Disposed;

        private void Dispose(bool dispose)
        {
            if (_Disposed || !dispose)
            {
                return;
            }

            _Disposed = true;
            Identified = false;

            Connection?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}