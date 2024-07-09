using System;

namespace Cortside.AmqpTools.DomainService.Models {
    /// <summary>
    /// Message Details
    /// </summary>
    public class AmqpToolsMessage {
        public object Body { get; set; }
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string PartitionKey { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string ContentType { get; set; }
        public DateTime? ScheduledEnqueueTimeUtc { get; set; }
        public object UserProperties { get; set; }
        public object SystemProperties { get; set; }
    }
}
