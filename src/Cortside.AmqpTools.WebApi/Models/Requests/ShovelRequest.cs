using System.ComponentModel.DataAnnotations;

namespace Cortside.AmqpTools.WebApi.Models.Requests {
    /// <summary>
    /// Model to shoves messages in a queue's dlq
    /// </summary>
    public class ShovelRequest {
        /// <summary>
        /// Max Count of messages to shovel
        /// </summary>
        [Required, Range(1, int.MaxValue)]
        public int? MaxCount { get; set; }
    }
}
