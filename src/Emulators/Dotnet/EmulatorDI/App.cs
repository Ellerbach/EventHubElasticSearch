using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using Appemulator.Models;
using System.Threading.Tasks;

namespace Appemulator
{
    /// <summary>
    /// App Emulator for EMP. It uses Dependency Injection, to be used as an example for all AXA applications
    /// It will allow to inject Nlog targets or log4Net appenders in a transparent way
    /// For now focus on using EventHub SDK directly
    /// </summary>
    public class App
    {
        // private readonly ITestService _testService;
        private readonly ILogEmulator _logEmulator;
        private readonly ILogger<App> _logger;
        private readonly AppSettings _config;

        public App(ILogEmulator logEmulator,
          IOptions<AppSettings> config,
          ILogger<App> logger)
        {
            _logEmulator = logEmulator;
            _logger = logger;
            _config = config.Value;
        }

        public async void Run()
        {
            _logger.LogInformation($"This is a console application for {_config.ConsoleTitle}");
            _logEmulator.Run();

            string myEmpSampleData = File.ReadAllText(_config.EmpSampleData);
            ICollection<LogEntry> myJsonObject = JsonConvert.DeserializeObject<ICollection<LogEntry>>(myEmpSampleData);
            Random rnd = new Random();

            // Create a producer client that you can use to send events to an event hub
            await using (
                var producerClient = new EventHubProducerClient(_config.EmpConnectionString, _config.EmpEventHubName))
            {
                while (!Console.KeyAvailable)
                {
                    // Wait for a random period of time
                    await Task.Delay(rnd.Next(5) * 1000);

                    // Create a batch of events 
                    using EventDataBatch eventBatch = producerClient.CreateBatchAsync().Result;

                    foreach (var logEntry in myJsonObject)
                    {
                        // Setting now date to facilitate view in the Kibana dashboards
                        logEntry.date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string x = JsonConvert.SerializeObject(logEntry);
                        // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
                        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(x)));
                    }

                    // Use the producer client to send the batch of events to the event hub
                    await producerClient.SendAsync(eventBatch);
                    _logger.LogInformation($"A batch of {myJsonObject.Count} events has been published.");
                }
            }
        }
    }
}
