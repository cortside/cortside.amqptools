using System.Linq;
using System.Reflection;
using Cortside.AmqpTools.WebApi.Mappers;
using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cortside.AmqpTools.WebApi.Installers {
    /// <summary>
    /// Installer for model mappers
    /// </summary>
    public class ModelMapperInstaller : IInstaller {
        /// <summary>
        /// Installs model mappers
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public void Install(IServiceCollection services, IConfiguration configuration) {
            typeof(QueueModelMapper).GetTypeInfo().Assembly.GetTypes()
                .Where(x => x.Name.EndsWith("Mapper")
                    && x.GetTypeInfo().IsClass
                    && !x.GetTypeInfo().IsAbstract)
                .ToList().ForEach(x => {
                    services.AddScoped(x);
                });
        }
    }
}
