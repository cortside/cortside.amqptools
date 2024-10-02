using System.ComponentModel.DataAnnotations;
using Cortside.AmqpTools.Dto.Enumerations;

namespace Cortside.AmqpTools.Dto.Dto {
    public class DeleteMessageRequestDto {
        [Required]
        public MessageType? MessageType { get; set; }

        [Required, MinLength(1)]
        public string MessageId { get; set; }
    }
}
