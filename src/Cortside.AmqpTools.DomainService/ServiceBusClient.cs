using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Amqp;
using Amqp.Framing;
using Cortside.AmqpTools.Configuration;
using Cortside.AmqpTools.DomainService.Models;
using Cortside.AmqpTools.DomainService.Models.Responses;
using Cortside.AmqpTools.Exceptions;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cortside.AmqpTools.DomainService {
    public class ServiceBusClient : IServiceBusClient {
        private readonly AmqpToolsConfiguration config;
        private readonly ILogger<ServiceBusClient> logger;
        const string MESSAGE_TYPE_KEY = "Message.Type.FullName";

        public ServiceBusClient(AmqpToolsConfiguration config, ILogger<ServiceBusClient> logger) {
            this.config = config;
            this.logger = logger;
        }

        private ManagementClient GetClient() => new ManagementClient(config.ConnectionString);

        public async Task<MessageCountDetailsResponse> GetQueueAsync(string queue) {
            var managementClient = GetClient();
            var messages = await managementClient.GetQueueRuntimeInfoAsync(queue);
            return CountDetailsToModel(messages.MessageCountDetails);
        }

        public async Task<List<AmqpToolsMessage>> PeekMessagesAsync(MessageReceiver receiver, int count) {
            var messages = await receiver.PeekAsync(count);
            var amqpServiceMessages = new List<AmqpToolsMessage>();

            foreach (var message in messages) {
                amqpServiceMessages.Add(MessageToModel(message));
            }

            return amqpServiceMessages;
        }

        public void Shovel(int count, string targetQueue, string sourceQueue) {
            AmqpConnection conn = null;
            try {
                conn = Connect();
                ReceiverLink receiver = new ReceiverLink(conn.Session, "receiver-drain", sourceQueue);

                Message message;
                int nReceived = 0;
                receiver.SetCredit(config.InitialCredit);
                TimeSpan timeSpan = TimeSpan.FromSeconds(10);
                while ((message = receiver.Receive(timeSpan)) != null) {
                    nReceived++;
                    logger.LogInformation("Message(Properties={Properties}, ApplicationProperties={ApplicationProperties}, Body={Body}", message.Properties, message.ApplicationProperties, message.Body);
                    Send(message, targetQueue);
                    receiver.Accept(message);
                    if (config.Max > 0 && nReceived == count) {
                        logger.LogInformation("max messages received");
                        break;
                    }
                }
                if (message == null) {
                    logger.LogInformation("No message");
                }
                receiver.Close();
                conn.Session.Close();
                conn.Connection.Close();
            } catch (Exception e) {
                logger.LogError(e, "Exception {Message}.", e.Message);
                if (null != conn?.Connection) {
                    conn.Connection.Close();
                }
                throw new AmqpShovelException($"Unable to shovel messages. {e}");
            }
        }

        public async Task DeleteMessageAsync(string queue, string sourceQueue, string messageId) {
            logger.LogInformation("Attempting to delete message {MessageId}", messageId);
            var counts = await GetQueueAsync(queue).ConfigureAwait(false);
            var count = sourceQueue.Contains("deadletter", StringComparison.CurrentCultureIgnoreCase) ? counts.DeadLetterMessageCount : counts.ActiveMessageCount;
            AmqpConnection conn = null;
            List<Message> messages = new List<Message>();
            Message message;
            try {
                conn = Connect();
                ReceiverLink receiver = new ReceiverLink(conn.Session, $"receiver-read", sourceQueue);
                var success = false;
                receiver.SetCredit((int)count);
                TimeSpan timeSpan = TimeSpan.FromSeconds(10);
                while ((message = await receiver.ReceiveAsync(timeSpan)) != null) {
                    logger.LogInformation("Reading message {MessageId}", message.Properties.MessageId);
                    if (message.Properties.MessageId == messageId) {
                        receiver.Accept(message);
                        success = true;
                        logger.LogInformation("Successfully deleted message {MessageId}", message.Properties.MessageId);
                    } else {
                        messages.Add(message);
                    }
                }
                logger.LogInformation("releasing {Count} messages", messages.Count);
                foreach (var msg in messages) {
                    receiver.Release(msg);
                }
                if (!success) {
                    throw new ResourceNotFoundMessage($"Message {messageId} could not be found.");
                }

                logger.LogInformation("Closing connection");
                await receiver.CloseAsync();
                await conn.Session.CloseAsync();
                await conn.Connection.CloseAsync();
                logger.LogInformation("Connection closed");
            } catch (Exception e) {
                if (null != conn?.Connection) {
                    await conn.Connection.CloseAsync();
                }
                throw new AmqpShovelException("Exception deleting messages.", e);
            }
        }

        private void Send(Message message, string queue) {
            var conn = Connect();

            var attach = new Attach() {
                Target = new Target() { Address = queue, Durable = Convert.ToUInt32(config.Durable) },
                Source = new Source()
            };
            var sender = new SenderLink(conn.Session, "shovel", attach, null);

            string rawBody = null;
            // Get the body
            try {
                if (message.Body is string) {
                    rawBody = message.Body as string;
                } else if (message.Body is byte[]) {
                    using (var reader = XmlDictionaryReader.CreateBinaryReader(
                        new MemoryStream(message.Body as byte[]),
                        null,
                        XmlDictionaryReaderQuotas.Max)) {
                        var doc = new XmlDocument();
                        doc.Load(reader);
                        rawBody = doc.InnerText;
                    }
                }
            } catch (Exception ex) {
                logger.LogError(ex, "ServiceBusClient failed to parse message {MessageId} with body {Body}.", message.Properties.MessageId, message.Body);
                throw new AmqpConnectionMessage($"ServiceBusClient failed to parse message {message.Properties.MessageId}.");
            }

            // duplicate message so that original can be ack'd
            var m = new Message(rawBody) {
                Header = message.Header,
                ApplicationProperties = message.ApplicationProperties,
                Properties = message.Properties
            };

            logger.LogInformation("publishing message {MessageId} to {Queue} with event type {ApplicationProperties} with body:\n{Body}", message.Properties.MessageId, queue, message.ApplicationProperties[MESSAGE_TYPE_KEY], rawBody);
            try {
                sender.Send(m);
                logger.LogInformation("successfully published message {MessageId}", message.Properties.MessageId);
            } finally {
                if (sender.Error != null) {
                    logger.LogError("ERROR: [{Condition}] {Description}", sender.Error.Condition, sender.Error.Description);
                }
                if (!sender.IsClosed) {
                    sender.Close(TimeSpan.FromSeconds(5));
                }
                conn.Session.Close();
                conn.Session.Connection.Close();
            }
            if (sender.Error != null) {
                throw new AmqpException(sender.Error);
            }
        }

        public AmqpConnection Connect() {
            logger.LogInformation("Connecting to {Url}.", config.Url);
            try {
                Address address = new Address(config.Url);
                var connection = new Connection(address);
                Session session = new Session(connection);

                logger.LogInformation($"Connection successfully established.");
                return new AmqpConnection() { Connection = connection, Session = session };
            } catch (Exception ex) {
                logger.LogError(ex, "ServiceBusClient failed to establish connection.");
                throw new AmqpConnectionMessage();
            }
        }

        private MessageCountDetailsResponse CountDetailsToModel(MessageCountDetails messageCount) {
            return new MessageCountDetailsResponse() {
                ActiveMessageCount = messageCount.ActiveMessageCount,
                DeadLetterMessageCount = messageCount.DeadLetterMessageCount,
                ScheduledMessageCount = messageCount.ScheduledMessageCount,
                TransferMessageCount = messageCount.TransferMessageCount,
                TransferDeadLetterMessageCount = messageCount.TransferDeadLetterMessageCount
            };
        }

        private void CloseConnection(AmqpConnection connection) {
            try {
                if (connection != null) {
                    connection.Session.Close();
                    connection.Connection.Close();
                }
            } catch (Exception e) {
                logger.LogInformation("AmqpTools failed to close a connection and responded with {Message}.", e);
            }

        }

        private AmqpToolsMessage MessageToModel(Microsoft.Azure.ServiceBus.Message message) {
            return new AmqpToolsMessage() {
                Body = JsonConvert.DeserializeObject(message.GetBody<string>()),
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                ContentType = message.ContentType,
                PartitionKey = message.PartitionKey,
                ExpiresAtUtc = message.ExpiresAtUtc,
                ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc,
                UserProperties = message.UserProperties,
                SystemProperties = message.SystemProperties
            };
        }
    }
}
