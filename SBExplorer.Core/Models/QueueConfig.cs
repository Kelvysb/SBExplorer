namespace SBExplorer.Core.Models
{
    public class QueueConfig
    {
        public string Key { get; set; }

        public string QueuePath { get; set; }

        public string QueueName { get; set; }

        public string Description { get; set; }

        public bool NotifyChanges { get; set; }

        public string LastMessage { get; set; }

        public string DefaultMessage { get; set; }
    }
}