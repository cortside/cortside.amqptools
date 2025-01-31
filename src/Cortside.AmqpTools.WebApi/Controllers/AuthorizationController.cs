using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cortside.AmqpTools.WebApi.Models.Responses;
using Cortside.Authorization.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cortside.AmqpTools.WebApi.Controllers {
    /// <summary>
    /// provides resources from the policy server
    /// </summary>
    [Route("api/v{version:apiVersion}/authorization")]
    [ApiController]
    [ApiVersion("1")]
    [Produces("application/json")]
    [Authorize]
    public class AuthorizationController : ControllerBase {
        private readonly ILogger<AuthorizationController> logger;
        private readonly IAuthorizationApiClient policyClient;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Authorization controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        /// <param name="configuration"></param>
        public AuthorizationController(ILogger<AuthorizationController> logger, IAuthorizationApiClient client, IConfiguration configuration) {
            this.logger = logger;
            this.policyClient = client;
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the list if permissions associated with the caller, determined by their bearer token
        /// </summary>
        /// <returns>The list of permissions</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(AuthorizationModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPermissionsAsync() {
            logger.LogInformation("Retrieving authorization permissions for user.");
            var authProperties = await policyClient.EvaluatePolicyAsync(new Authorization.Client.Models.EvaluationDto { User = User });
            AuthorizationModel responseModel = new AuthorizationModel() {
                Permissions = authProperties.Permissions.ToList()
            };
            var permissionsPrefix = configuration.GetSection("AuthorizationApi").GetValue<string>("PolicyName");
            responseModel.Permissions = responseModel.Permissions.Select(p => $"{permissionsPrefix}.{p}").ToList();
            return Ok(responseModel);
        }
    }
}
