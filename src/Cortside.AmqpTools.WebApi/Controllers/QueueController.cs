using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService;
using Cortside.AmqpTools.WebApi.Mappers;
using Cortside.AmqpTools.WebApi.Models.Requests;
using Cortside.AmqpTools.WebApi.Models.Responses;
using Cortside.AspNetCore.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cortside.AmqpTools.WebApi.Controllers {

    /// <summary>
    /// Represents the commands available to perform on amqp queues
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/queues")]
    [Authorize]
    public class QueueController : Controller {
        private readonly ILogger logger;
        private readonly IServiceBusService service;
        private readonly QueueModelMapper mapper;

        /// <summary>
        /// Initializes a new instance of the AmqpToolsController
        /// </summary>
        public QueueController(ILogger<QueueController> logger,
            QueueModelMapper mapper,
            IServiceBusService service) {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        /// <summary>
        /// Get runtime details of a queue
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        [HttpGet("{queue}")]
        [Authorize(Constants.Authorization.Permissions.GetQueues)]
        [ProducesResponseType(typeof(QueueRuntimeInfoResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetQueueRuntimeInfoAsync(string queue) {
            logger.LogInformation("Getting runtime info for queue {Queue}", queue);
            var result = await service.GetMessageDetailsByQueueAsync(queue);
            return Ok(mapper.Map(result));
        }



        /// <summary>
        /// Peeks messages for queue by type (active, deadletter)
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="peekRequestModel"></param>
        /// <returns></returns>
        [HttpGet("{queue}/peek")]
        [Authorize(Constants.Authorization.Permissions.GetQueues)]
        [ProducesResponseType(typeof(ListResult<MessageResponse>), 200)]
        public async Task<IActionResult> GetMessagesByQueueAsync(string queue, [FromQuery] PeekRequest peekRequestModel) {
            var result = await service.PeekMessagesAsync(queue, mapper.Map(peekRequestModel));
            return Ok(new ListResult<MessageResponse>(result.ToList().ConvertAll(x => mapper.Map(x))));
        }

        /// <summary>
        /// Shovel max count of messages for queue's deadletterqueue
        /// </summary>
        /// <param name="queue">The queue</param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("{queue}/shovel")]
        [Authorize(Constants.Authorization.Permissions.ShovelQueues)]
        [ProducesResponseType(202)]
        public async Task<IActionResult> ShovelQueueAsync(string queue, [FromBody] ShovelRequest requestModel) {
            await service.ShovelMessagesAsync(queue, requestModel.MaxCount);
            return Accepted();
        }

        /// <summary>
        /// Delete a message from queue by type (active, deadletter)
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        [HttpDelete("{queue}/message")]
        [Authorize(Constants.Authorization.Permissions.DeleteMessage)]
        [ProducesResponseType(202)]
        public async Task<IActionResult> DeleteMessageFromQueueAsync(string queue, [FromBody] DeleteMessageRequest requestBody) {
            await service.DeleteMessageAsync(queue, mapper.Map(requestBody)).ConfigureAwait(false);
            return Accepted();
        }
    }
}
