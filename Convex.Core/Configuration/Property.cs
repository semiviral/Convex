namespace Convex.Core.Configuration
{
    public class Property : IProperty
    {
        public Property(string property, object value)
        {
            Key = property;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }

        public override string ToString() => (string)Value;
    }
}
