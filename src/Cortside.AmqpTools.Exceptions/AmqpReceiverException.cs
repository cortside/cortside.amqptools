using Cortside.Common.Messages.MessageExceptions;

namespace Cortside.AmqpTools.Exceptions {
    public class AmqpReceiverException : InternalServerErrorResponseException {
        public AmqpReceiverException() : base("Failed to receive messages.") { }

        public AmqpReceiverException(string message) : base(message) {
        }

        public AmqpReceiverException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
