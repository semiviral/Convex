namespace Convex.Core.Net {
    public class Address : IAddress {
        public Address(string hostname, int port) {
            Hostname = hostname;
            Port = port;
        }

        #region MEMBERS

        public string Hostname { get; }
        public int Port { get; }

        #endregion

        public override string ToString() {
            return $"{Hostname}:{Port}";
        }
    }
}