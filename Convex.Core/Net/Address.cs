namespace Convex.Core.Net
{
    public class Address : IAddress
    {
        public Address(string hostname, int port)
        {
            Hostname = hostname.Trim();
            Port = port;
        }

        public override string ToString()
        {
            return $"{Hostname}:{Port}";
        }

        #region MEMBERS

        public string Hostname { get; }
        public int Port { get; }

        #endregion
    }
}