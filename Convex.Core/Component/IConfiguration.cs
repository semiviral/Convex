using System.Collections.Generic;

namespace Convex.Core {
    public interface IConfiguration {
        Dictionary<string, string> ApiKeys { get; }
        string DatabaseFilePath { get; set; }
        string FilePath { get; set; }
        List<string> IgnoreList { get; }
        string LogFilePath { get; set; }
        string PluginsDirectoryPath { get; set; }
        string Nickname { get; set; }
        string Password { get; set; }
        string Realname { get; set; }

        void Dispose();
        IConfiguration ParseConfig(string config);
    }
}