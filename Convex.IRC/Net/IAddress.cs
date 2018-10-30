namespace Convex.IRC.Net {
    public interface IAddress {
        string Hostname { get; }
        int Port { get; }
    }
}