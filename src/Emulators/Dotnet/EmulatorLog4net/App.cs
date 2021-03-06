﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Appemulator.Models;
using log4net;
using log4net.Config;
using System.Reflection;

namespace Appemulator
{
    /// <summary>
    /// App Emulator for EMP. It references the Nlog target from AXA's internal Nuget feed
    /// </summary>
    public class App
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// App Emulator Main method with no expected args
        /// </summary>
        /// <returns></returns>
        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            await emulateLog4netTarget(config);
        }

        private static async Task emulateLog4netTarget(IConfiguration config)
        {
            string myEmpSampleData = File.ReadAllText(config["sampledata"]);
            ICollection<LogEntry> myJsonObject = JsonConvert.DeserializeObject<ICollection<LogEntry>>(myEmpSampleData);
            Random rnd = new Random();

            while (!Console.KeyAvailable)
            {
                foreach (var logEntry in myJsonObject)
                {
                    // Wait for a random period of time
                    await Task.Delay(rnd.Next(5) * 1000);

                    log.Info(logEntry.message);
                    log.Debug(logEntry.message);
                    log.Warn(logEntry.message);
                    log.Error(logEntry.message);
                }
                Console.WriteLine($"A batch of {myJsonObject.Count * 4} events has been published.");
            }
        }
    }
}
