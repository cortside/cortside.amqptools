//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Cortside.AmqpTools.Configuration;
//using Cortside.AmqpTools.DomainService.Models;
//using Cortside.AmqpTools.DomainService.Models.Responses;
//using Cortside.AmqpTools.Dto.Enumerations;
//using Cortside.AmqpTools.Exceptions;
//using FluentAssertions;
//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Xunit;
//using Xunit.Abstractions;

//namespace Cortside.AmqpTools.DomainService.Tests {
//    public class ServiceBusServiceTest : DomainServiceTest<IServiceBusService> {
//        private ServiceBusService ServiceBusService { get; }

//        private readonly Mock<IServiceBusClient> clientMock;
//        private readonly MessageCountDetailsResponse messageCountDetailsResponse;

//        public ServiceBusServiceTest(ITestOutputHelper testOutputHelper) : base() {
//            Mock<ILogger<ServiceBusService>> loggerMock = new();
//            clientMock = new Mock<IServiceBusClient>();

//            var config = new ConfigurationBuilder()
//                .AddJsonFile("appsettings.json")
//                .Build();
//            var serviceConfig = config.GetSection("Options").Get<AmqpToolsConfiguration>();

//            ServiceBusService = new ServiceBusService(
//                loggerMock.Object,
//                serviceConfig,
//                clientMock.Object);

//            messageCountDetailsResponse = new MessageCountDetailsResponse() {
//                ActiveMessageCount = 5,
//                DeadLetterMessageCount = 4,
//                ScheduledMessageCount = 3,
//                TransferDeadLetterMessageCount = 2,
//                TransferMessageCount = 1
//            };

//            clientMock.Setup(c => c.GetQueueAsync(It.IsAny<string>())).Returns(Task.FromResult(messageCountDetailsResponse));
//            clientMock.Setup(c => c.PeekMessagesAsync(It.IsAny<MessageReceiver>(), It.IsAny<int>())).Returns(Task.FromResult(new List<AmqpToolsMessage>() {
//                new AmqpToolsMessage(){ Body = new MessageBody(){ Id = Guid.NewGuid().ToString() }, MessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString(), PartitionKey = "queue1.name.event" },
//                new AmqpToolsMessage(){ Body = new MessageBody(){ Id = Guid.NewGuid().ToString() }, MessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString(), PartitionKey = "queue2.name.event" },
//                new AmqpToolsMessage(){ Body = new MessageBody(){ Id = Guid.NewGuid().ToString() }, MessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString(), PartitionKey = "queue3.name.event" },
//            }));
//            clientMock.Setup(c => c.Shovel(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
//        }

//        [Fact]
//        public async Task ShouldReturnMessageCountDetailsAsync() {
//            //arrange

//            //act
//            var response = await ServiceBusService.GetMessageDetailsByQueueAsync("queue");

//            //assert
//            response.ActiveMessageCount.Should().Be(messageCountDetailsResponse.ActiveMessageCount);
//            response.DeadLetterMessageCount.Should().Be(messageCountDetailsResponse.DeadLetterMessageCount);
//            response.ScheduledMessageCount.Should().Be(messageCountDetailsResponse.ScheduledMessageCount);
//            response.TransferDeadLetterMessageCount.Should().Be(messageCountDetailsResponse.TransferDeadLetterMessageCount);
//            response.TransferMessageCount.Should().Be(messageCountDetailsResponse.TransferMessageCount);
//        }

//        [Theory]
//        [InlineData("active", MessageType.Active)]
//        [InlineData("deadletter", MessageType.DeadLetter)]
//        [InlineData("scheduled", MessageType.Scheduled)]
//        [InlineData("DeadLetter", MessageType.DeadLetter)]
//        public async Task ShouldReturnCountByTypeAsync(string messageType, MessageType expectedResult) {
//            //arrange

//            //act
//            var response = await ServiceBusService.GetMessageDetailsByQueueAsync("queue", messageType);

//            //assert
//            switch (expectedResult) {
//                case MessageType.Active:
//                    response.Count.Should().Be(messageCountDetailsResponse.ActiveMessageCount);
//                    break;
//                case MessageType.DeadLetter:
//                    response.Count.Should().Be(messageCountDetailsResponse.DeadLetterMessageCount);
//                    break;
//                case MessageType.Scheduled:
//                    response.Count.Should().Be(messageCountDetailsResponse.ScheduledMessageCount);
//                    break;
//                default:
//                    throw new InvalidOperationException($"{messageType} could not be mapped to a MessageType.");
//            }
//        }

//        [Fact]
//        public async Task ShouldThrowExceptionInvalidMessageTypeAsync() {
//            //arrange

//            //act
//            try {
//                await ServiceBusService.GetMessageDetailsByQueueAsync("queue", "invalid");
//                throw new InvalidOperationException("Expected InvalidArgumentMessage, but no exception was thrown.");
//            } catch (Exception ex) {
//                //assert
//                ex.GetType().Should().Be(typeof(InvalidArgumentMessage));
//            }
//        }

//        [Fact]
//        public async Task ShouldPeekMessagesAsync() {
//            //arrange

//            //act
//            var response = await ServiceBusService.PeekMessagesAsync("queue", "deadletter", 2);

//            //assert
//            response.Count.Should().Be(3);
//            foreach (var r in response) {
//                r.Body.Should().NotBeNull();
//                r.MessageId.Should().NotBeNull();
//                r.CorrelationId.Should().NotBeNull();
//            }
//        }

//        [Fact]
//        public async Task PeekMessagesShouldThrowExceptionOnScheduledQueueAsync() {
//            //arrange

//            //act
//            try {
//                await ServiceBusService.PeekMessagesAsync("queue", "scheduled", 2);
//                throw new InvalidOperationException("Expected InvalidArgumentMessage, but no exception was thrown.");
//            } catch (Exception ex) {
//                //assert
//                ex.GetType().Should().Be(typeof(InvalidArgumentMessage));
//            }
//        }

//        [Fact]
//        public async Task ShouldShovelMessagesAsync() {
//            //arrange
//            var queue = "queue";

//            //act
//            await ServiceBusService.ShovelMessagesAsync(queue);

//            //assert
//            clientMock.Verify(x => x.Shovel(4, queue, EntityNameHelper.FormatDeadLetterPath(queue)), Times.Once);
//        }

//        [Fact]
//        public async Task ShouldDeleteMessageAsync() {
//            //arrange
//            var queue = "queue";
//            var messageId = Guid.NewGuid().ToString();
//            var type = "deadletter";

//            //act
//            await ServiceBusService.DeleteMessageAsync(queue, type, messageId);

//            //assert
//            clientMock.Verify(x => x.DeleteMessageAsync(queue, EntityNameHelper.FormatDeadLetterPath(queue), messageId), Times.Once);
//        }

//        public class MessageBody {
//            public string Id { get; set; }
//            public string InternalId { get; set; }
//        }
//    }
//}
