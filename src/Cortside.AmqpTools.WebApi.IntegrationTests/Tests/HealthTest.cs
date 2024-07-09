using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Cortside.AmqpTools.WebApi.IntegrationTests.Tests {
    public class HealthTest : IClassFixture<TestWebApplicationFactory<Startup>> {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly HttpClient testServerClient;

        public HealthTest(TestWebApplicationFactory<Startup> fixture, ITestOutputHelper testOutputHelper) {
            this.testOutputHelper = testOutputHelper;
            testServerClient = fixture.CreateClient(new WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task TestAsync() {
            //arrange

            //act
            HttpResponseMessage response = await testServerClient.GetAsync("api/health");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            var respStr = (await response.Content.ReadAsStringAsync());

            dynamic obj = JObject.Parse(respStr);

            string status = obj.status;
            status.Should().Be("failure");
            bool healthy = obj.healthy;
            healthy.Should().BeFalse();
        }
    }
}
