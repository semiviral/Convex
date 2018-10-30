using System.Collections.Generic;

namespace Convex.IRC {
    public interface IConfiguration {
        Dictionary<string, string> ApiKeys { get; }
        string DatabaseFilePath { get; set; }
        string FilePath { get; set; }
        List<string> IgnoreList { get; }
        string LogFilePath { get; set; }
        string Nickname { get; set; }
        string Password { get; set; }
        string Realname { get; set; }

        void Dispose();
    }
}