using Xunit.Abstractions;

namespace Cortside.AmqpTools.DomainService.Tests {
    public class ServiceBusServiceTest : DomainServiceTest<IServiceBusService> {

        /// <summary>
        /// Relying on integration test for each endpoint, add units as needed
        /// </summary>
        /// <param name="testOutputHelper"></param>
        public ServiceBusServiceTest(ITestOutputHelper testOutputHelper) : base() {



        }

    }
}
