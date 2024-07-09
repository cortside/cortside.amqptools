using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cortside.AmqpTools.WebApi.Controllers {

    /// <summary>
    /// Represents the shared functionality/resources of the AmqpTools resource
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [ApiController]
    [Route("api/v{version:apiVersion}/amqptools")]
    [Authorize]
    public class AmqpController : Controller {
        private readonly ILogger logger;
        private readonly IServiceBusService service;

        /// <summary>
        /// Initializes a new instance of the AmqpToolsController
        /// </summary>
        public AmqpController(ILogger<AmqpController> logger, IServiceBusService service) {
            this.logger = logger;
            this.service = service;
        }

        /// <summary>
        /// Get all details by queue
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        [HttpGet("{queue}")]
        [Authorize(Constants.Authorization.Permissions.GetQueues)]
        [ProducesResponseType(typeof(MessageCountDetailsResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessageCountByQueueAsync(string queue) {
            var result = await service.GetMessageDetailsByQueueAsync(queue);
            return Ok(result);
        }

        /// <summary>
        /// Gets message count for queue by type
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="type">MessageType (active, deadletter, scheduled)</param>
        /// <returns></returns>
        [HttpGet("{queue}/{type}")]
        [Authorize(Constants.Authorization.Permissions.GetQueues)]
        [ProducesResponseType(typeof(QueueCount), 200)]
        public async Task<IActionResult> GetMessageCountByQueueByTypeAsync(string queue, string type) {
            var result = await service.GetMessageDetailsByQueueAsync(queue, type);
            return Ok(result);
        }

        /// <summary>
        /// Gets messages for queue by type
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="type">MessageType (active, deadletter, scheduled)</param>
        /// <param name="count">Number of messages to peek</param>
        /// <returns></returns>
        [HttpGet("{queue}/{type}/{count}")]
        [Authorize(Constants.Authorization.Permissions.GetQueues)]
        [ProducesResponseType(typeof(List<AmqpToolsMessage>), 200)]
        public async Task<IActionResult> GetMessagesByQueueAsync(string queue, string type, int count) {
            var result = await service.PeekMessagesAsync(queue, type, count);
            return Ok(result);
        }

        /// <summary>
        /// Shovel all messages for queue
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        [HttpPost("{queue}")]
        [Authorize(Constants.Authorization.Permissions.ShovelQueues)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ShovelQueueAsync(string queue) {
            await service.ShovelMessagesAsync(queue);
            return Accepted();
        }

        /// <summary>
        /// Delete a message from queue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="type"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost("{queue}/{type}/{messageId}")]
        [Authorize(Constants.Authorization.Permissions.DeleteMessage)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteMessageFromQueueAsync(string queue, string type, string messageId) {
            await service.DeleteMessageAsync(queue, type, messageId).ConfigureAwait(false);
            return Accepted();
        }
    }
}
