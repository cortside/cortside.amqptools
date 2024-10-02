using System.ComponentModel.DataAnnotations;
using Cortside.AmqpTools.Dto.Enumerations;

namespace Cortside.AmqpTools.Dto.Dto {
    /// <summary>
    /// Model to peek at messages in a queue
    /// </summary>
    public class PeekRequestDto {
        /// <summary>
        /// Queue type (active, deadletter)
        /// </summary>
        [Required]
        public MessageType? MessageType { get; set; }

        /// <summary>
        /// Count of messages to peek
        /// </summary>
        [Required, MinLength(1)]
        public int? Count { get; set; }
    }
}
