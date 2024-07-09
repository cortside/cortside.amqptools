using System;
using System.Runtime.Serialization;
using Cortside.Common.Messages.MessageExceptions;

namespace Cortside.AmqpTools.Exceptions {
    public class ResourceNotFoundMessage : NotFoundResponseException {
        public ResourceNotFoundMessage() : base("Resource could not be found.") { }

        public ResourceNotFoundMessage(string message) : base(message) {
        }

        public ResourceNotFoundMessage(string message, Exception exception) : base(message, exception) {
        }

        protected ResourceNotFoundMessage(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
