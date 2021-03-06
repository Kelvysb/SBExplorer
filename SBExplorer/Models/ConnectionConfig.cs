using System.Collections.Generic;

namespace SBExplorer.Models
{
    public class ConnectionConfig
    {
        public string BaseJsonPath { get; set; }

        public string Key { get; set; }

        public string Endpoint { get; set; }

        public string SharedKeyName { get; set; }

        public string SharedKey { get; set; }

        public string Description { get; set; }

        public bool ConnectionstringFullPathMode { get; set; }

        public string ConnectionString { get; set; }

        public string QueueNamesPattern { get; set; }

        public string QueueNamesContains { get; set; }

        public string QueueNamesNotContains { get; set; }

        public List<QueueConfig> Queues { get; set; }

        public bool AutoCreateMissingQueues { get; set; }

        public bool AutoConnect { get; set; }

        public bool Isolated { get; set; } = false;
    }
}
