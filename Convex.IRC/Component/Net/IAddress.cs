namespace Convex.IRC.Component.Net {
    public interface IAddress {
        string Hostname { get; }
        int Port { get; }
    }
}