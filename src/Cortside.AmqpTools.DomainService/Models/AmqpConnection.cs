using Amqp;

namespace Cortside.AmqpTools.DomainService.Models {
    /// <summary>
    /// Amqp Connection Details
    /// </summary>
    public class AmqpConnection {
        public Connection Connection { get; set; }
        public Session Session { get; set; }
    }
}
