using System.ComponentModel.DataAnnotations;
using Cortside.AmqpTools.Dto.Enumerations;

namespace Cortside.AmqpTools.WebApi.Models.Requests {
    /// <summary>
    /// Model to delete a message in a queue, by type
    /// </summary>
    public class DeleteMessageRequest {
        /// <summary>
        /// Message type (active, deadletter)
        /// </summary>
        [Required]
        public MessageType? MessageType { get; set; }

    }
}
