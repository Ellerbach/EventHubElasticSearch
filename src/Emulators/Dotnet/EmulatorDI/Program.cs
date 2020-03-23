using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nlog = NLog;
using NLog.Extensions.Logging;
using log4net;
using log4net.Config;
using Appemulator.Models;
using Appemulator.Services;

namespace Appemulator
{
    /// <summary>
    /// Starting point for the App Emulator. Uses Dependency Injection to include Loggers and appSettings reading
    /// </summary>
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// App Emulator Main method with no expected args
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                // Load log4net configuration
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                // create service collection
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                // create service provider
                var serviceProvider = serviceCollection.BuildServiceProvider();

                // run app
                serviceProvider.GetService<App>().Run();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Nlog.LogManager.Shutdown();
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // build configuration
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();

            serviceCollection.AddLogging(configure =>
            {
                configure
                  .AddFilter("Microsoft", LogLevel.Warning)
                  .AddFilter("System", LogLevel.Warning)
                  .AddFilter("Appemulator.Program", LogLevel.Debug)
                  .AddConsole()
                  .AddEventLog()
                  .AddDebug()
                  .AddNLog(configuration);
            });

            serviceCollection.AddSingleton<IConfiguration>(configuration);

            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            ConfigureConsole(configuration);


            // add services
            serviceCollection.AddTransient<ILogEmulator, LogEmulator>();

            // add app
            serviceCollection.AddTransient<App>();

        }

        private static void ConfigureConsole(IConfigurationRoot configuration)
        {
            System.Console.Title = configuration.GetSection("Configuration:ConsoleTitle").Value;
        }
    }
}