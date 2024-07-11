using AmqpTools.Core.Models;
using Cortside.AmqpTools.Dto.Dto;
using Cortside.AmqpTools.WebApi.Models.Requests;
using Cortside.AmqpTools.WebApi.Models.Responses;
using Newtonsoft.Json;

namespace Cortside.AmqpTools.WebApi.Mappers {
    /// <summary>
    /// Mapper for Queue models
    /// </summary>
    public class QueueModelMapper {
        internal PeekRequestDto Map(PeekRequest peekRequestModel) {
            return new PeekRequestDto {
                MessageType = peekRequestModel.MessageType,
                Count = peekRequestModel.Count
            };
        }

        internal QueueRuntimeInfoResponse Map(AmqpToolsQueueRuntimeInfo result) {
            return new QueueRuntimeInfoResponse {
                Path = result.Path,
                MessageCount = result.MessageCount,
                MessageCountDetails = Map(result.MessageCountDetails),
                SizeInBytes = result.SizeInBytes,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                AccessedAt = result.AccessedAt
            };
        }

        internal MessageCountDetailsResponse Map(AmqpToolsMessageCountDetails messageCountDetails) {
            return new MessageCountDetailsResponse {
                ActiveMessageCount = messageCountDetails.ActiveMessageCount,
                DeadLetterMessageCount = messageCountDetails.DeadLetterMessageCount,
                ScheduledMessageCount = messageCountDetails.ScheduledMessageCount,
                TransferMessageCount = messageCountDetails.TransferMessageCount,
                TransferDeadLetterMessageCount = messageCountDetails.TransferDeadLetterMessageCount
            };
        }

        internal MessageResponse Map(AmqpToolsMessage message) {
            return new MessageResponse {
                Body = JsonConvert.DeserializeObject(message.Body.ToString()),
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                PartitionKey = message.PartitionKey,
                ExpiresAtUtc = message.ExpiresAtUtc,
                ContentType = message.ContentType,
                ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc,
                UserProperties = message.UserProperties,
                SystemProperties = message.SystemProperties
            };
        }

        internal DeleteMessageRequestDto Map(string messageId, DeleteMessageRequest requestBody) {
            return new DeleteMessageRequestDto {
                MessageType = requestBody.MessageType,
                MessageId = messageId
            };
        }
    }
}
