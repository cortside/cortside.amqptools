using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;

namespace Cortside.AmqpTools.DomainService {
    public interface IServiceBusService {
        Task<MessageCountDetailsResponse> GetMessageDetailsByQueueAsync(string queue);
        Task<QueueCount> GetMessageDetailsByQueueAsync(string queue, string messageType);
        Task ShovelMessagesAsync(string queue);
        Task<IList<AmqpToolsMessage>> PeekMessagesAsync(string queue, string messageType, int count);
        Task DeleteMessageAsync(string queue, string type, string messageId);
    }
}
