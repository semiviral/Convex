namespace Convex.Plugin.Composition {
    public class CompositionDescription {
        public CompositionDescription(string command, string description) {
            Command = command;
            Description = description;
        }

        public string Command { get; }
        public string Description { get; }
    }
}
