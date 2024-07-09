using Cortside.Common.Messages.MessageExceptions;

namespace Cortside.AmqpTools.Exceptions {
    public class InvalidArgumentMessage : BadRequestResponseException {
        public InvalidArgumentMessage() : base("The provided argument was not valid.") { }

        public InvalidArgumentMessage(string message) : base(message) {
        }

        public InvalidArgumentMessage(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
