using System.Collections.Generic;

namespace Convex.Configuration {
    public interface IConfig {
        string Host { get; }

        void Initialise(string basePath);
        void CommitConfiguration();
        IEnumerable<IProperty> GetProperties();
        IProperty GetProperty(string key);
        void Dispose();
    }
}