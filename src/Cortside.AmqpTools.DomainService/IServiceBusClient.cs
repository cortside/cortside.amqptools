using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Microsoft.Azure.ServiceBus.Core;

namespace Cortside.AmqpTools.DomainService {
    public interface IServiceBusClient {
        Task<MessageCountDetailsResponse> GetQueueAsync(string queue);
        Task<List<AmqpToolsMessage>> PeekMessagesAsync(MessageReceiver receiver, int count);
        void Shovel(int count, string targetQueue, string sourceQueue);
        Task DeleteMessageAsync(string queue, string sourceQueue, string messageId);
    }
}
