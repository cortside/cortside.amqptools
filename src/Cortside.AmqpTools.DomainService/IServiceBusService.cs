using System.Collections.Generic;
using System.Threading.Tasks;
using AmqpTools.Core.Models;
using Cortside.AmqpTools.Dto.Dto;

namespace Cortside.AmqpTools.DomainService {
    public interface IServiceBusService {
        Task<AmqpToolsQueueRuntimeInfo> GetMessageDetailsByQueueAsync(string queue);
        Task ShovelMessagesAsync(string queue, ShovelRequestDto dto);
        Task<IList<AmqpToolsMessage>> PeekMessagesAsync(string queue, PeekRequestDto dto);
        Task DeleteMessageAsync(string queue, DeleteMessageRequestDto dto);
    }
}
