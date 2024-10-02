using Cortside.AmqpTools.Configuration;
using Cortside.AmqpTools.DomainService;
using AmqpTools.Core;
using AmqpTools.Core.Commands;
using Cortside.AspNetCore.AccessControl;
using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cortside.AmqpTools.WebApi.Installers {
    /// <summary>
    /// Startup Installer
    /// </summary>
    public class StartupInstaller : IInstaller {
        /// <summary>
        /// Installs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Install(IServiceCollection services, IConfiguration configuration) {
            var authConfig = configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();

            Cortside.RestApiClient.Authenticators.OpenIDConnect.TokenRequest authenticationTokenRequest =
                new Cortside.RestApiClient.Authenticators.OpenIDConnect.TokenRequest {
                    AuthorityUrl = authConfig.Authority,
                    ClientId = authConfig.Authentication.ClientId,
                    ClientSecret = authConfig.Authentication.ClientSecret,
                    GrantType = authConfig.Authentication.GrantType,
                    Scope = authConfig.Authentication.Scope,
                    SlidingExpiration = authConfig.Authentication.SlidingExpiration
                };

            services.AddSingleton(authenticationTokenRequest);

            AmqpToolsConfiguration serviceConfiguration = configuration.GetSection("Options").Get<AmqpToolsConfiguration>();
            services.AddSingleton(serviceConfiguration);

            BaseOptions amqpToolsBaseOptions = configuration.GetSection("Options").Get<BaseOptions>();
            services.AddSingleton(amqpToolsBaseOptions);


            services.AddSingleton<ISubjectService, SubjectService>();
            services.AddSingleton<IAmqpToolsCore, AmqpToolsCore>();
        }
    }
}
