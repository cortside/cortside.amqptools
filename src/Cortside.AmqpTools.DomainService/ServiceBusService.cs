using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AmqpTools.Configuration;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Cortside.AmqpTools.Dto.Enumerations;
using Cortside.AmqpTools.Exceptions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cortside.AmqpTools.DomainService {
    public class ServiceBusService : IServiceBusService {
        private readonly ILogger<ServiceBusService> logger;
        private readonly AmqpToolsConfiguration config;
        private readonly IServiceBusClient client;

        public ServiceBusService(
            ILogger<ServiceBusService> logger,
            AmqpToolsConfiguration config,
            IServiceBusClient client) {
            this.logger = logger;
            this.config = config;
            this.client = client;
        }

        public async Task<MessageCountDetailsResponse> GetMessageDetailsByQueueAsync(string queue) {
            MessageCountDetailsResponse messageCount = await client.GetQueueAsync(queue);
            logger.LogInformation("MessageCountDetails for {Queue}: {Count}", queue, JsonConvert.SerializeObject(messageCount));

            return messageCount;
        }

        public async Task<QueueCount> GetMessageDetailsByQueueAsync(string queue, string messageType) {
            var messageCount = await GetMessageDetailsByQueueAsync(queue);
            logger.LogInformation("MessageCountDetails for {Queue}: {Count}", queue, JsonConvert.SerializeObject(messageCount));

            try {
                Enum.TryParse(typeof(MessageType), messageType, true, out object type);
                switch (type) {
                    case MessageType.Active:
                        return new QueueCount() { Count = messageCount.ActiveMessageCount };
                    case MessageType.DeadLetter:
                        return new QueueCount() { Count = messageCount.DeadLetterMessageCount };
                    case MessageType.Scheduled:
                        return new QueueCount() { Count = messageCount.ScheduledMessageCount };
                    default:
                        logger.LogError("Unexpected message type: {MessageType}", messageType);
                        throw new InvalidArgumentMessage($"Unrecognized message type {messageType}.");
                }
            } catch (Exception ex) {
                logger.LogError(ex, "Unable to parse message type: {MessageType}.", messageType);
                throw new InvalidArgumentMessage($"Unrecognized message type {messageType}.");
            }
        }

        public async Task<IList<AmqpToolsMessage>> PeekMessagesAsync(string queue, string messageType, int count) {
            string formattedQueue = FormatQueue(queue, messageType);
            logger.LogInformation("Peeking {Count} messages from {FormattedQueue}.", count, formattedQueue);

            var receiver = new MessageReceiver(config.ConnectionString, formattedQueue, ReceiveMode.PeekLock);

            // Browse messages from queue
            var messages = await client.PeekMessagesAsync(receiver, count);
            await receiver.CloseAsync();
            return messages;
        }

        public async Task ShovelMessagesAsync(string queue) {
            var dlq = EntityNameHelper.FormatDeadLetterPath(queue);
            var max = config.Max;
            logger.LogInformation("Connecting to {Queue} to shovel maximum of {Max} messages", queue, max);

            if (config.ConnectionString != null) {
                var messages = await GetMessageDetailsByQueueAsync(queue, MessageType.DeadLetter.ToString());
                logger.LogInformation("Message queue {Dlq} has {Count} messages", dlq, messages.Count);

                if (messages.Count < config.Max) {
                    max = Convert.ToInt32(messages.Count);
                    logger.LogInformation("Resetting max messages to {Max}", max);
                }
            }

            client.Shovel(max, queue, dlq);
            logger.LogInformation("ShovelMessages complete");
        }

        public Task DeleteMessageAsync(string queue, string type, string messageId) {
            string formattedQueue = FormatQueue(queue, type);
            logger.LogInformation("Connecting to {FormattedQueue} to delete message {MessageId}", formattedQueue, messageId);
            return client.DeleteMessageAsync(queue, formattedQueue, messageId);
        }

        private string FormatQueue(string queue, string messageType) {
            Enum.TryParse(typeof(MessageType), messageType, true, out object type);
            switch (type) {
                case MessageType.Active:
                    return queue;
                case MessageType.DeadLetter:
                    return EntityNameHelper.FormatDeadLetterPath(queue);
                default:
                    // Not supported
                    throw new InvalidArgumentMessage($"Peeking queues by type {messageType} is not supported");
            }
        }
    }
}
