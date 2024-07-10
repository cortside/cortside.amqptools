using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.Auth;
using Cortside.AmqpTools.WebApi.Models.Responses;
using Cortside.AspNetCore.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Cortside.AmqpTools.WebApi.IntegrationTests.Tests {
    public class QueueControllerTests : IClassFixture<TestWebApplicationFactory<Startup>> {
        private readonly HttpClient testServerClient;

        public QueueControllerTests(TestWebApplicationFactory<Startup> fixture) {
            testServerClient = fixture.CreateClient(new WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task ShouldGetMessageCountsAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = new Authentication().GetBearerToken();

            //act
            var response = await testServerClient.GetAsync("api/v1/queues/someQueue");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<QueueRuntimeInfoResponse>(contentString);
            responseData.MessageCountDetails.ActiveMessageCount.Should().Be(1);
            responseData.MessageCountDetails.DeadLetterMessageCount.Should().Be(2);
            responseData.MessageCountDetails.ScheduledMessageCount.Should().Be(3);
        }


        [Fact]
        public async Task ShouldPeekMessagesAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = new Authentication().GetBearerToken();

            //act
            var response = await testServerClient.GetAsync("api/v1/queues/someQueue/peek?count=10&messageType=deadletter");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var contentString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ListResult<MessageResponse>>(contentString);
            responseData.Results.Should().NotBeNullOrEmpty();
            responseData.Results.Should().HaveCount(2);
            foreach (var r in responseData.Results) {
                r.Body.Should().NotBeNull();
                r.MessageId.Should().NotBeNull();
                r.CorrelationId.Should().NotBeNull();
            }
        }
    }
}
