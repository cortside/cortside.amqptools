using Cortside.AmqpTools.DomainService;
using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cortside.AmqpTools.BootStrap.Installers {
    public class DomainServiceInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddTransient<IServiceBusService, ServiceBusService>();
        }
    }
}
