#region USINGS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Convex.Configuration {
    public static class Config {
        public static void WriteConfig() {
            using (StreamWriter wStream = new StreamWriter($@"{AppContext.BaseDirectory}\config\config")) {
                foreach (Property prop in _properties) {
                    wStream.WriteLineAsync($"{prop}");
                }
            }
        }

        #region MEMBERS

        private static List<IProperty> _properties = new List<IProperty>();

        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public const string DEFAULT_CONFIG = "\r\n";
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

        public static IEnumerable<IProperty> GetProperties() {
            return _properties;
        }

        public static void CommitConfiguration() {
            WriteConfig();
        }

        public static void Initialise(string basePath) {
            string fullPath = $@"{basePath}\config";

            // if file doesn't exist, create it
            // and write defaults to it
            if (!File.Exists(fullPath)) {
                File.Create(fullPath);
                File.WriteAllText(fullPath, DEFAULT_CONFIG);
            }

            // read properties from file
            using (StreamReader sReader = new StreamReader(fullPath)) {
                while (!sReader.EndOfStream) {
                    string rawProp = sReader.ReadLine();

                    // For comments
                    if (rawProp.StartsWith('#')) continue;

                    string[] splitProp = rawProp.Split(' ', 2);
                    _properties.Add(new Property(splitProp[0], splitProp[1]));
                }
            }
        }

        public static IProperty GetProperty(string key) {
            return _properties.Single(prop => prop.Key.Equals(key));
        }

        #endregion
    }
}