using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AmqpTools.Dto.Dto;
using AmqpTools.Core.Models;

namespace Cortside.AmqpTools.DomainService {
    public interface IServiceBusService {
        Task<AmqpToolsQueueRuntimeInfo> GetMessageDetailsByQueueAsync(string queue);
        Task ShovelMessagesAsync(string queue, int? maxCount);
        Task<IList<AmqpToolsMessage>> PeekMessagesAsync(string queue, PeekRequestDto dto);
        Task DeleteMessageAsync(string queue, DeleteMessageRequestDto dto);
    }
}
