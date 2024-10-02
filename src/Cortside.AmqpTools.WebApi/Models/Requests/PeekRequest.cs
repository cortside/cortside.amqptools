using System.ComponentModel.DataAnnotations;
using Cortside.AmqpTools.Dto.Enumerations;

namespace Cortside.AmqpTools.WebApi.Models.Requests {
    /// <summary>
    /// Model to peek at messages in a queue
    /// </summary>
    public class PeekRequest {
        /// <summary>
        /// Queue type (active, deadletter)
        /// </summary>
        [Required]
        public MessageType? MessageType { get; set; }

        /// <summary>
        /// Count of messages to peek
        /// </summary>
        [Required, Range(1, int.MaxValue)]
        public int? Count { get; set; }
    }
}
