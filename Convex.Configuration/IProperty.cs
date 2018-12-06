namespace Convex.Configuration {
    public interface IProperty {
        string Key { get; }
        object Value { get; }

        string ToString();
    }
}