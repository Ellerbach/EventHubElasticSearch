using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Common;

namespace Nlog.Eventhub
{
    /// <summary>
    /// Nlog target implementation using guidance from official documentation.
    /// PartitionKey is optional. If no partition key is supplied the log messages are sent to eventhub
    /// and distributed to all partitions (round robin).
    /// </summary>
    [Target("YourPartition")]
    public class EventHubNlogTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string EventHubConnectionString { get; set; }

        [RequiredParameter]
        public string EventHubName { get; set; }

        public string EventHubTransportType { get; set; }

        /// <summary>
        /// PartitionKey is optional. If no partition key is supplied the log messages are sent to eventhub
        /// and distributed to various partitions in a round robin manner.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Takes the contents of the LogEvent and sends the message to EventHub
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            SendAsync(PartitionKey, logEvent);
        }

        private async Task<bool> SendAsync(string partitionKey, LogEventInfo logEvent) //AsyncLogEventInfo
        {
            string curEventHubConnectionString, curEventHubName, curEventHubTransportTypeConfig;
            EventHubsTransportType curEventHubTransportType;

            try
            {
                curEventHubConnectionString = EventHubConnectionString ?? Environment.GetEnvironmentVariable("NL_CONNECTION_STRING");
                curEventHubName = EventHubName ?? Environment.GetEnvironmentVariable("NL_NAME");
                curEventHubTransportType = EventHubsTransportType.AmqpWebSockets; // Default value
                curEventHubTransportTypeConfig = EventHubTransportType ?? Environment.GetEnvironmentVariable("NL_TRANSPORT_TYPE");
                curEventHubTransportType = (EventHubsTransportType)Enum.Parse(typeof(EventHubsTransportType), curEventHubTransportTypeConfig);

                string logMessage = this.Layout.Render(logEvent);
                InternalLogger.Debug($"EventHubNlogTarget:Starting SendAsync {logMessage}");

                EventHubProducerClientOptions ehOptions = new EventHubProducerClientOptions()
                {
                    ConnectionOptions = new EventHubConnectionOptions { TransportType = EventHubsTransportType.AmqpWebSockets }
                };

                await using (var producerClient = new EventHubProducerClient(curEventHubConnectionString, curEventHubName, ehOptions))
                {
                    var batchOptions = new CreateBatchOptions { PartitionKey = partitionKey };

                    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync(batchOptions);

                    eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(logMessage)));

                    await producerClient.SendAsync(eventBatch);
                    InternalLogger.Debug("EventHubNlogTarget:Sent to Azure EventHub!");
                }
            }
            catch (ArgumentNullException) 
            {
                throw new ArgumentException("EMP Nlog Eventhub missing TransportType configuration. Default is AmqpWebSockets.");
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("EMP Nlog Eventhub incorrectly configured. Invalid TransportType. Allowed values are AmqpTcp or AmqpWebSockets. Default is AmqpWebSockets.");
            }
            catch (Exception ex)
            {
                InternalLogger.Trace($"EventHubNlogTarget: Failed to send : {ex.Message} : {ex.StackTrace}");
                throw ex;
            }
            return true;
        }
    }
}

