using System.Collections.Generic;
using System.Threading.Tasks;
using AmqpTools.Core;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using AmqpTools.Core.Models;
using Cortside.AmqpTools.Dto.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cortside.AmqpTools.DomainService {
    public class ServiceBusService : IServiceBusService {
        private readonly ILogger<ServiceBusService> logger;
        private readonly IAmqpToolsCore toolsCore;
        private readonly BaseOptions baseOptions;

        public ServiceBusService(
            ILogger<ServiceBusService> logger,
            IAmqpToolsCore amqpToolsCore,
             BaseOptions amqpToolsBaseOptions
            ) {
            this.logger = logger;
            toolsCore = amqpToolsCore;
            baseOptions = amqpToolsBaseOptions;
        }

        public async Task<AmqpToolsQueueRuntimeInfo> GetMessageDetailsByQueueAsync(string queue) {
            var options = new QueueOptions {
                Queue = queue,
                Namespace = baseOptions.Namespace,
                Key = baseOptions.Key,
                Durable = baseOptions.Durable,
                PolicyName = baseOptions.PolicyName,
                Protocol = baseOptions.Protocol,
                InitialCredit = baseOptions.InitialCredit,
                Timeout = baseOptions.Timeout,
            };

            var result = toolsCore.GetQueueRuntimeInfo(options);

            logger.LogInformation("Queue runtime info for {Queue}: {Result}", queue, JsonConvert.SerializeObject(result));
            await Task.CompletedTask;

            return result;
        }



        public async Task<IList<AmqpToolsMessage>> PeekMessagesAsync(string queue, PeekRequestDto dto) {

            var options = new PeekOptions {
                Queue = queue,
                MessageType = dto.MessageType.ToString(),
                Count = dto.Count ?? 10,
                Namespace = baseOptions.Namespace,
                Key = baseOptions.Key,
                Durable = baseOptions.Durable,
                PolicyName = baseOptions.PolicyName,
                Protocol = baseOptions.Protocol,
                InitialCredit = baseOptions.InitialCredit,
                Timeout = baseOptions.Timeout,
            };

            logger.LogInformation("Connecting to {Queue} to peek {Count} messages of type {MessageType}", queue, options.Count, options.MessageType);
            var messages = toolsCore.PeekMessages(options);
            await Task.CompletedTask;
            logger.LogInformation("PeekMessages complete");

            return messages;
        }

        public async Task ShovelMessagesAsync(string queue, ShovelRequestDto dto) {
            var options = new ShovelOptions {
                Queue = queue,
                Namespace = baseOptions.Namespace,
                Max = dto.MaxCount,
                MessageId = dto.MessageId,
                Key = baseOptions.Key,
                Durable = baseOptions.Durable,
                PolicyName = baseOptions.PolicyName,
                Protocol = baseOptions.Protocol,
                InitialCredit = baseOptions.InitialCredit,
                Timeout = baseOptions.Timeout,
            };

            logger.LogInformation("Connecting to {Queue} to shovel maximum of {Max} messages", queue, options.Max);
            toolsCore.ShovelMessages(options);
            await Task.CompletedTask;
            logger.LogInformation("ShovelMessages complete");
        }

        public Task DeleteMessageAsync(string queue, DeleteMessageRequestDto dto) {
            var options = new DeleteMessageOptions {
                Queue = queue,
                MessageId = dto.MessageId,
                MessageType = dto.MessageType.ToString(),
                Namespace = baseOptions.Namespace,
                Key = baseOptions.Key,
                Durable = baseOptions.Durable,
                PolicyName = baseOptions.PolicyName,
                Protocol = baseOptions.Protocol,
                InitialCredit = baseOptions.InitialCredit,
                Timeout = baseOptions.Timeout,
            };


            logger.LogInformation("Connecting to {Queue} to delete message {MessageId} for type {MessageType}", options.Queue, dto.MessageId, dto.MessageType);
            toolsCore.DeleteMessage(options);
            return Task.CompletedTask;
        }


    }
}
