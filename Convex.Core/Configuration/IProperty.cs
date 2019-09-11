namespace Convex.Core.Configuration
{
    public interface IProperty
    {
        string Key { get; }
        object Value { get; }

        string ToString();
    }
}
