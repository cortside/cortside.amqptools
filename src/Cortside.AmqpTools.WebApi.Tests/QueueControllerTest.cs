//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Cortside.AmqpTools.DomainService;
//using Cortside.AmqpTools.DomainService.Models.Responses;
//using Cortside.AmqpTools.WebApi.Controllers;
//using Cortside.AmqpTools.WebApi.Models.Responses;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.ServiceBus.Management;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Xunit;

//namespace Cortside.AmqpTools.WebApi.Tests {
//    public class QueueControllerTest : IDisposable {
//        private readonly Mock<IServiceBusService> serviceMock;
//        private readonly Mock<ILogger<QueueController>> loggerMock;
//        private readonly QueueController controller;
//        protected UnitTestFixture TestFixture { get; set; }


//        public QueueControllerTest() {
//            TestFixture = new UnitTestFixture();
//            serviceMock = TestFixture.Mock<IServiceBusService>();
//            loggerMock = TestFixture.Mock<ILogger<QueueController>>();
//            controller = new QueueController(loggerMock.Object, new Mappers.QueueModelMapper(), serviceMock.Object);
//        }

//        [Fact]
//        public async Task ShouldGetAllMessageCountsByQueueAsync() {
//            //arrange
//            var expectedResponse = new QueueRuntimeInfo {
//                MessageCountDetails = new MessageCountDetails() {
//                    ActiveMessageCount = 1,
//                    DeadLetterMessageCount = 2,
//                    ScheduledMessageCount = 3,
//                    TransferDeadLetterMessageCount = 4,
//                    TransferMessageCount = 5
//                }
//            };

//            serviceMock.Setup(s => s.GetMessageDetailsByQueueAsync("queue")).ReturnsAsync(expectedResponse);

//            //act
//            var result = await controller.GetQueueRuntimeInfoAsync("queue");

//            //assert
//            var viewResult = Assert.IsAssignableFrom<OkObjectResult>(result);
//            viewResult.Value.Should().BeEquivalentTo(expectedResponse);
//        }



//        [Theory]
//        [InlineData("deadletter")]
//        [InlineData("active")]
//        public async Task ShouldPeekMessagesAsync(string type) {
//            //arrange
//            string queue = "queue";
//            int count = 100;

//            IList<AmqpToolsMessage> expectedResponse = new List<AmqpToolsMessage>() {
//                new AmqpToolsMessage() {
//                    Body = new MessageBody() {
//                        Id = Guid.NewGuid().ToString(),
//                        InternalId = Guid.NewGuid().ToString()
//                    },
//                    MessageId = Guid.NewGuid().ToString()
//                },
//                new AmqpToolsMessage() {
//                    Body = new MessageBody() {
//                        Id = Guid.NewGuid().ToString(),
//                        InternalId = Guid.NewGuid().ToString()
//                    },
//                    MessageId = Guid.NewGuid().ToString()
//                }
//            };
//            serviceMock.Setup(s => s.PeekMessagesAsync(queue, type, count)).Returns(Task.FromResult(expectedResponse));

//            //act
//            var result = await controller.GetMessagesByQueueAsync(queue, type, count);

//            //assert
//            var viewResult = Assert.IsAssignableFrom<OkObjectResult>(result);
//            viewResult.Value.Should().BeEquivalentTo(expectedResponse);
//        }

//        [Theory]
//        [InlineData("deadletter")]
//        [InlineData("active")]
//        public async Task ShouldDeleteMessageAsync(string type) {
//            //arrange
//            string queue = "queue";
//            string messageId = Guid.NewGuid().ToString();

//            serviceMock.Setup(s => s.DeleteMessageAsync(queue, type, messageId)).Returns(Task.CompletedTask);

//            //act
//            var result = await controller.DeleteMessageFromQueueAsync(queue, type, messageId);

//            //assert
//            Assert.IsAssignableFrom<AcceptedResult>(result);
//            serviceMock.Verify(s => s.DeleteMessageAsync(queue, type, messageId), Times.Once);
//        }

//        public class MessageBody {
//            public string Id { get; set; }
//            public string InternalId { get; set; }
//        }

//        public void Dispose() {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//        protected virtual void Dispose(bool disposing) {
//            TestFixture.TearDown();
//            if (disposing) {
//                controller.Dispose();
//            }
//        }
//    }
//}
