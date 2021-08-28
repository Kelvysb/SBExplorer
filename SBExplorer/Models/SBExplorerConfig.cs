namespace SBExplorer.Models
{
    public class SBExplorerConfig
    {
        public ConfigFile ConfigFile { get; set; }

        public bool DefaultAutoCreateMissingQueues { get; set; }

        public bool DefaultAutoConnect { get; set; }
    }
}
