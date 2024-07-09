using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AmqpTools.DomainService;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.IDSMock;
using Cortside.AmqpTools.WebApi.IntegrationTests.Helpers.WireMock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Cortside.AmqpTools.WebApi.IntegrationTests {
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class {
        public Mock<IServiceBusClient> ServiceBusClientMock { get; set; }
        private IdsMock idsMock;
        private readonly IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.integration.json", optional: false, reloadOnChange: false)
            .AddInMemoryCollection(
                new Dictionary<string, string> {
                    ["IdentityServer:ApiSecret"] = Guid.NewGuid().ToString(),
                    ["IdentityServer:Authentication:ClientSecret"] = Guid.NewGuid().ToString(),
                    ["Encryption:Secret"] = Guid.NewGuid().ToString()
                })
            .Build();

        protected override IHostBuilder CreateHostBuilder() {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "integration");

            var fluentMockServer = new BaseWireMock();
            this.idsMock = new IdsMock(fluentMockServer.MockServer);

            var authConfig = configuration.GetSection("IdentityServer");
            authConfig["Authority"] = idsMock.MockServer.Urls[0];
            authConfig["BaseUrl"] = $"{idsMock.MockServer.Urls[0]}/connect/token";
            authConfig["RequireHttpsMetadata"] = "false";

            var policyServerConfig = configuration.GetSection("PolicyServer");
            var policyserverTokenClient = policyServerConfig.GetSection("TokenClient");
            policyserverTokenClient["Authority"] = fluentMockServer.MockServer.Urls[0];
            policyServerConfig["PolicyServerUrl"] = fluentMockServer.MockServer.Urls[0];

            ServiceBusClientMock = new Mock<IServiceBusClient>();
            ServiceBusClientMock.Setup(c => c.GetQueueAsync(It.IsAny<string>())).Returns(Task.FromResult(new MessageCountDetailsResponse() {
                ActiveMessageCount = 1,
                DeadLetterMessageCount = 2,
                ScheduledMessageCount = 3
            }));
            ServiceBusClientMock.Setup(c => c.PeekMessagesAsync(It.IsAny<MessageReceiver>(), It.IsAny<int>())).Returns(Task.FromResult(new List<AmqpToolsMessage>() {
                new AmqpToolsMessage() {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    PartitionKey = "someQueue.someEvent",
                    Body = new MessageBody() { Id = Guid.NewGuid().ToString(), InternalId = Guid.NewGuid().ToString() }
                },
                new AmqpToolsMessage() {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    PartitionKey = "someQueue.someEvent",
                    Body = new MessageBody() { Id = Guid.NewGuid().ToString(), InternalId = Guid.NewGuid().ToString() }
                }
            }));

            return Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(builder => {
                        builder.AddConfiguration(configuration);
                    })
                  .ConfigureWebHostDefaults(webbuilder => {
                      webbuilder
                      .UseConfiguration(configuration)
                      .UseStartup<TStartup>()
                      .ConfigureTestServices(sc => {
                          sc.AddSingleton(ServiceBusClientMock.Object);
                          sc.AddSingleton(services => new DiagnosticContext(Log.Logger));
                          sc.AddSingleton<IDiagnosticContext>(services => services.GetRequiredService<DiagnosticContext>());
                      })
                      .ConfigureLogging(x => x.AddSerilog(Log.Logger));

                  });
        }

        public class MessageBody {
            public string Id { get; set; }
            public string InternalId { get; set; }
        }
    }
}
