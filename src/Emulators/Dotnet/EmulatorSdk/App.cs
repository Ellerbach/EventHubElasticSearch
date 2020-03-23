using Appemulator.Models;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Appemulator
{
    /// <summary>
    /// App Emulator. It uses Azure EventHub SDK directly
    /// </summary>
    public class App
    {
        /// <summary>
        /// App Emulator Main method with no expected args
        /// </summary>
        /// <returns></returns>
        static async Task Main()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            await EmulateSDK(config);
        }

        static async Task EmulateSDK(IConfiguration config)
        {
            string myEmpSampleData = File.ReadAllText(config["sampledata"]);
            ICollection<LogEntry> myJsonObject = JsonConvert.DeserializeObject<ICollection<LogEntry>>(myEmpSampleData);
            Random rnd = new Random();

            // Create a producer client that you can use to send events to an event hub
            await using (var producerClient = new EventHubProducerClient(config["connectionString"], config["eventHubName"]))
            {
                while (!Console.KeyAvailable)
                {
                    // Wait for a random period of time
                    await Task.Delay(rnd.Next(5) * 1000);

                    // Create a batch of events 
                    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                    foreach (var logEntry in myJsonObject)
                    {
                        // Setting now date to facilitate view in the Kibana dashboards
                        logEntry.date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string logEntryString = JsonConvert.SerializeObject(logEntry);
                        // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
                        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(logEntryString)));
                    }

                    // Use the producer client to send the batch of events to the event hub
                    await producerClient.SendAsync(eventBatch);
                    Console.WriteLine($"A batch of {myJsonObject.Count} events has been published.");
                }

            }
        }
    }
}
