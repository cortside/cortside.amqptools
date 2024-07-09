using Cortside.Common.BootStrap;
using Cortside.AmqpTools.DomainService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cortside.AmqpTools.BootStrap.Installers {
    public class DomainServiceInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddTransient<IServiceBusService, ServiceBusService>();
            services.AddTransient<IServiceBusClient, ServiceBusClient>();
        }
    }
}
