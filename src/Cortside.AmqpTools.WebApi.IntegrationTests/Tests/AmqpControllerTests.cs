using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Cortside.AmqpTools.WebApi.IntegrationTests.Tests {
    public class AmqpControllerTests : IClassFixture<TestWebApplicationFactory<Startup>> {
        private readonly HttpClient testServerClient;

        public AmqpControllerTests(TestWebApplicationFactory<Startup> fixture) {
            testServerClient = fixture.CreateClient(new WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task ShouldGetMessageCountsAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = new Authentication().GetBearerToken();

            //act
            var response = await testServerClient.GetAsync("api/v1/AmqpTools/queue");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<MessageCountDetailsResponse>(contentString);
            responseData.ActiveMessageCount.Should().Be(1);
            responseData.DeadLetterMessageCount.Should().Be(2);
            responseData.ScheduledMessageCount.Should().Be(3);
        }

        [Fact]
        public async Task ShouldGetCountByQueueAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = new Authentication().GetBearerToken();

            //act
            var response = await testServerClient.GetAsync("api/v1/AmqpTools/queue/deadletter");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<QueueCount>(contentString);
            responseData.Count.Should().Be(2);
        }

        [Fact]
        public async Task ShouldPeekMessagesAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = new Authentication().GetBearerToken();

            //act
            var response = await testServerClient.GetAsync("api/v1/AmqpTools/queue/deadletter/10");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<List<AmqpToolsMessage>>(contentString);
            responseData.Count.Should().Be(2);
            foreach (var r in responseData) {
                r.Body.Should().NotBeNull();
                r.MessageId.Should().NotBeNull();
                r.CorrelationId.Should().NotBeNull();
            }
        }
    }
}
