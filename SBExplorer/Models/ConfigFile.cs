using System.Collections.Generic;

namespace SBExplorer.Models
{
    public class ConfigFile
    {
        public ConfigFileSource Source { get; set; }

        public List<ConnectionConfig> Connections { get; set; }

        public string FilePath { get; set; }
    }
}
