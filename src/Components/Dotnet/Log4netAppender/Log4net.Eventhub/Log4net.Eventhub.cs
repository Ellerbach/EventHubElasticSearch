using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Microsoft.Azure.EventHubs;
using log4net.Appender;
using log4net.Core;

namespace Log4net.Eventhub
{
    /// <summary>
    /// Class implementing Log4net custom Appender base class (AppenderSkeleton).
    /// This class implements the log4net appender sending data to Azure EventHubs
    /// All settings are done by configuration in the applications, accordingly to log4net best practices
    /// Configuration can also be set through environment variables
    /// </summary>
    public class Log4netEventHubAppender : AppenderSkeleton
    {
        public string EventHubConnectionString { get; set; }
        public string EventHubName { get; set; }
        public string EventHubTransportType { get; set; }
        public string ApplicationTrigram { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationLayer { get; set; }

        private EventHubClient eventHubClient;

        /// <summary>
        /// Method: ActivateOptions
        /// Goal: Active Log4net logger.
        /// </summary>
        /// <exception cref="ArgumentException">Invalid Transport Type in provided configuration</exception>
        public override void ActivateOptions()
        {
            string curEventHubConnectionString, curEventHubName, curEventHubTransportTypeConfig;
            TransportType curEventHubTransportType;

            try
            {
                curEventHubConnectionString = EventHubConnectionString ?? Environment.GetEnvironmentVariable("EMP_CONNECTION_STRING");
                curEventHubName = EventHubName ?? Environment.GetEnvironmentVariable("EMP_NAME");
                curEventHubTransportType = TransportType.AmqpWebSockets; // Default value
                curEventHubTransportTypeConfig = EventHubTransportType ?? Environment.GetEnvironmentVariable("EMP_TRANSPORT_TYPE");

                curEventHubTransportType = (TransportType)Enum.Parse(typeof(TransportType), curEventHubTransportTypeConfig);
            }
            catch
            {
                throw new ArgumentException("EMP Log4net Eventhub incorrectly configured. Invalid TransportType.");
            }

            // Creating Eventhub connection string
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(curEventHubConnectionString)
            {
                EntityPath = curEventHubName,
                TransportType = curEventHubTransportType
            };

            // Creating Eventhub client object
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

#if NETSTANDARD1_1
            Debug.Write("Target framework: .NET Standard 1.1");
elif NET40
            Debug.Write("Target framework: .NET Framework 4.0");
#elif NET45
            Debug.Write("Target framework: .NET Framework 4.5");
#elif NET462
            Debug.Write("Target framework: .NET Framework 4.6.2");
#else
            Debug.Write("Target framework: .NET Standard 2.0");
#endif

            base.ActivateOptions();
        }

        /// <summary>
        /// Method: Append
        /// Goal: Appends applicational logging event with additional metadata.
        /// </summary>
        /// <param name="loggingEvent">The logging event fired by the application</param>
        /// <exception cref="ArgumentException">Invalid Transport Type in provided configuration</exception>
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var eventData = SerializeEmpEvent(loggingEvent);

                JsonSerializerSettings empSettings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new LowercaseContractResolver()
                };
                empSettings.Converters.Add(new StringEnumConverter());

                var message = JsonConvert.SerializeObject(eventData, Formatting.Indented, empSettings);
                eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
            }
            catch (Exception e)
            {
                ErrorHandler.Error("Error occured: " + e);
            }
        }

        /// <summary>
        /// Method: SerializeEmpEvent
        /// Goal: Serialize logging event, ensure data formats and fields.
        /// </summary>
        /// <returns>Anonymous object</returns>        
        /// <param name="loggingEvent">The logging event fired by the application</param>
        private object SerializeEmpEvent(LoggingEvent loggingEvent)
        {
            var exceptionString = loggingEvent.GetExceptionString();
            if (string.IsNullOrWhiteSpace(exceptionString))
            {
                exceptionString = null;
            }
            return new
            {
                trigram = ApplicationTrigram,
                application = ApplicationName,
                layer = ApplicationLayer,
                level = loggingEvent.Level.DisplayName,
                date = loggingEvent.TimeStampUtc.ToString("o", CultureInfo.InvariantCulture),
                thread = loggingEvent.ThreadName,
                message = loggingEvent.MessageObject,
                ex = exceptionString
            };
        }
    }

    /// <summary>
    /// Class implementing a lower case contract resolver.
    /// Provides methods to be used during NewtonSoft.Json serialization operations
    /// </summary>
    public class LowercaseContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Method: ResolvePropertyName
        /// Goal: to transform any json property value to lower case.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>the lower case property value</returns>        
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}
