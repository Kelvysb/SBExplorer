namespace SBExplorer.Models
{
    public class QueueInfo
    {
        public long ActiveMessagesCount { get; set; }

        public long DeadLetterCount { get; set; }

        public long ScheduledMessagesCount { get; set; }

        public override string ToString()
        {
            return $"({ActiveMessagesCount}, {DeadLetterCount}, {ScheduledMessagesCount})";
        }
    }
}
