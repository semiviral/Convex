namespace Convex.Configuration {
    public class Property : IProperty {
        public Property(string property, object value) {
            Key = property;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }
    }
}