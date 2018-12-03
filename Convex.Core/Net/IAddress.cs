namespace Convex.Core.Net {
    public interface IAddress {
        string Hostname { get; }
        int Port { get; }

        string ToString();
    }
}