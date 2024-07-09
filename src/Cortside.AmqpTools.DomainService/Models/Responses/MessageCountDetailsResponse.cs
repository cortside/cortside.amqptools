namespace Cortside.AmqpTools.DomainService.Models.Responses {
    public class MessageCountDetailsResponse {
        public long ActiveMessageCount { get; set; }
        public long DeadLetterMessageCount { get; set; }
        public long ScheduledMessageCount { get; set; }
        public long TransferMessageCount { get; set; }
        public long TransferDeadLetterMessageCount { get; set; }
    }
}
