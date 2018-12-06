#region USINGS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Convex.Configuration {
    public class Config : IConfig {
        public Config(string host) {
            Host = host;
            _properties = new List<IProperty>();
        }

        public static void WriteConfig(string host, IEnumerable<IProperty> properties) {
            using (StreamWriter wStream = new StreamWriter($@"{AppContext.BaseDirectory}\config\{host}.conf")) {
                foreach (Property prop in properties) {
                    wStream.WriteLineAsync($"{prop}");
                }
            }
        }

        #region MEMBERS

        public string Host { get; }
        private List<IProperty> _properties;

        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public const string DEFAULT_CONFIG = "{\r\n\t\"IgnoreList\": [],\r\n\t\"ApiKeys\": { \"YouTube\": \"\", \"Dictionary\": \"\" },\r\n\t\"Realname\": \"Evealyn\",\r\n\t\"Nickname\": \"Eve\",\r\n\t\"Password\": \"evepass\",\r\n\t\"DatabaseFilePath\": \"\",\r\n\t\"LogFilePath\": \"\",\r\n\t\"PluginsDirectoryPath\": \"\"\r\n}\r\n";
        public static readonly string DefaultResourceDirectory = AppContext.BaseDirectory.EndsWith(@"\") ? $"{AppContext.BaseDirectory}Resources" : $@"{AppContext.BaseDirectory}\Resources";
        public static readonly string DefaultConfigurationFilePath = DefaultResourceDirectory + @"\config.json";
        public static readonly string DefualtDatabaseFilePath = DefaultResourceDirectory + @"\users.sqlite";
        public static readonly string DefaultLogFilePath = DefaultResourceDirectory + @"\Logged.txt";
        public static readonly string DefaultPluginDirectoryPath = DefaultResourceDirectory + @"\Plugins";

        public List<string> IgnoreList { get; } = new List<string>();
        public Dictionary<string, string> ApiKeys { get; } = new Dictionary<string, string>();

        public string FilePath { get; set; }

        public string Realname { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }

        public string LogFilePath {
            get => string.IsNullOrEmpty(_logFilePath) ? DefaultLogFilePath : _logFilePath;
            set => _logFilePath = value;
        }

        public string PluginDirectoryPath {
            get => string.IsNullOrEmpty(_databaseFilePath) ? DefualtDatabaseFilePath : _databaseFilePath;
            set => _databaseFilePath = value;
        }

        private string _databaseFilePath;
        private bool _disposed;
        private string _logFilePath;

        #endregion

        #region INTERFACE IMPLEMENTATION

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose) {
            if (!dispose || _disposed) {
                return;
            }

            WriteConfig(Host, _properties);

            _disposed = true;
        }

        public IEnumerable<IProperty> GetProperties() {
            return _properties;
        }

        public void CommitConfiguration() {

        }

        public void Initialise(string basePath) {
            string fullPath = $@"{basePath}\{Host}.conf";

            if (!File.Exists(fullPath)) {
                File.Create(fullPath);
            }

            using (StreamReader sReader = new StreamReader(fullPath)) {
                string[] rawProp = sReader.ReadLine().Split(' ', 2);
                Property prop = new Property(rawProp[0], rawProp[1]);

                _properties.Add(prop);
            }
        }

        public IProperty GetProperty(string key) {
            return _properties.Single(prop => prop.Key.Equals(key));
        }

        #endregion
    }
}