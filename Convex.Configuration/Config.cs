#region USINGS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Convex.Util;

#endregion

namespace Convex.Configuration {
    public static class Config {
        #region MEMBERS

        private static List<IProperty> _properties = new List<IProperty>();

        // I know this isn't readable. Just run the program once and you'll get a much cleaner
        // representation of the default config in the generated config.json
        public static readonly string DefaultConfig = $"Nickname Eve\r\nRealname evealyn\r\nPassword evepass\r\nIgnoreList eve\r\nPluginsDirectory {AppContext.BaseDirectory}\\plugins\\\r\nLogPath {AppContext.BaseDirectory}\\resource\\log.txt\r\n";

        #endregion

        #region INIT

        public static void Initialise(string basePath) {
            string fullPath = $@"{basePath}\config";

            StaticLog.Log(new Event.LogEventArgs(Serilog.Events.LogEventLevel.Information, "Loading configuration."));

            if (!Directory.Exists(basePath)) {
                StaticLog.Log(new Event.LogEventArgs(Serilog.Events.LogEventLevel.Error, "Base directory not found, creating."));

                Directory.CreateDirectory(basePath);
            }

            if (!File.Exists(fullPath)) {
                StaticLog.Log(new Event.LogEventArgs(Serilog.Events.LogEventLevel.Error, "Configuration file not found, creating with defaults."));

                using (StreamWriter wStream = new StreamWriter(File.Create(fullPath))) {
                    wStream.WriteLine(DefaultConfig);
                    wStream.Flush();
                }
            }

            // read properties from file
            using (StreamReader sReader = new StreamReader(fullPath)) {
                while (!sReader.EndOfStream) {
                    string rawProp = sReader.ReadLine();

                    // For comments
                    if (rawProp.StartsWith('#')) {
                        continue;
                    }

                    string[] splitProp = rawProp.Split(' ', 2);

                    if (splitProp.Length < 2) {
                        continue;
                    }

                    _properties.Add(new Property(splitProp[0], splitProp[1]));
                }
            }

            StaticLog.Log(new Event.LogEventArgs(Serilog.Events.LogEventLevel.Information, "Loaded configuration."));
        }

        #endregion

        #region METHODS

        private static void WriteConfig() {
            using (StreamWriter wStream = new StreamWriter($@"{AppContext.BaseDirectory}\config\config")) {
                foreach (Property prop in _properties) {
                    wStream.WriteLineAsync($"{prop}");
                }

                wStream.Flush();
            }
        }

        public static void CommitConfiguration() {
            StaticLog.Log(new Event.LogEventArgs(Serilog.Events.LogEventLevel.Information, "Committed configuration."));

            WriteConfig();
        }

        public static IEnumerable<IProperty> GetProperties() {
            return _properties;
        }

        public static IProperty GetProperty(string key) {
            if (!_properties.Select(prop => prop.Key).Contains(key)) {
                return null;
            }

            return _properties.Single(prop => prop.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}