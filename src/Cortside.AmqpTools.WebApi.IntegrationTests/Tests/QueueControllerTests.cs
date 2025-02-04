using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.Auth;
using Cortside.AmqpTools.WebApi.Models.Requests;
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
        public async Task ShouldGetQueueDetailsAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = Authentication.GetBearerToken();

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
            testServerClient.DefaultRequestHeaders.Authorization = Authentication.GetBearerToken();

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

        [Fact]
        public async Task ShouldShovelMessagesAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = Authentication.GetBearerToken();

            var body = new ShovelRequest {
                MaxCount = 5
            };

            //act
            var response = await testServerClient.PostAsJsonAsync("api/v1/queues/someQueue/shovel", body);

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task ShouldDeleteMessageAsync() {
            //get Token
            testServerClient.DefaultRequestHeaders.Authorization = Authentication.GetBearerToken();

            var request = new DeleteMessageRequest {
                MessageType = Dto.Enumerations.MessageType.DeadLetter
            };

            //act
            var response = await testServerClient.DeleteAsync($"api/v1/queues/someQueue/messages/{Guid.NewGuid}?messageType={request.MessageType}");

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }
    }
}
