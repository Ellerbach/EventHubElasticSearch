using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Queue;
using ElasticTransformation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Elasticsearch.Net;

namespace ElasticTransformation
{

    /// <summary>
    /// Class used to implement the main entry point for the function emp_logging_transformation:
    ///     1. Task Run with the EventHub trigger
    /// </summary>
    public static class Transformation
    {
        /// <summary>
        /// Main entry point for the function making the transformation, checking validity of the various elements
        /// </summary>
        /// <param name="events">Events from Event Hub to process</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <param name="executionContext">The execution context</param>
        /// <returns>Completed task on success</returns>
        [FunctionName("logging_transformation")]
        public static async Task Run([EventHubTrigger("%EMP_EVENT_HUB_NAME%", Connection = "EMP_EVENTHUB_CONNECTION_STRING")] EventData[] events, ILogger log, ExecutionContext executionContext)
        {
            var exceptions = new List<Exception>();
            string infoMessage;
            string errorMessage;

            string jsonSettingsPath = executionContext.FunctionAppDirectory;

            //App Settings from local.settings.json
            string storageConnectionString = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_STORAGE_ACCOUNT_CONNECTION_STRING;
            string teamsQueueName = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_STORAGE_ACCOUNT_TEAMS_QUEUE_NAME;
            string errorQueueName = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_STORAGE_ACCOUNT_ERROR_QUEUE_NAME;
            string trigramTableName = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_TRIGRAM_TABLE_NAME;
            string elasticSearchClusterURI = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_ELASTIC_SEARCH_CLUSTER_URI;
            string elasticSearchAPIId = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_ELASTIC_SEARCH_API_ID;
            string elasticSearchAPIKey = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_ELASTIC_SEARCH_API_KEY;

            foreach (EventData eventData in events)
            {
                JsonLogEntry logEntry = null;
                string webhookUrl = null;
                bool validTrigram = false;
                string messageBody = null;

                try
                {
                    messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    log?.LogInformation($"FROM EVENTHUB {messageBody}");
                    if (string.IsNullOrEmpty(messageBody))
                    {
                        infoMessage = $"Task Run: Event is null";
                        log?.LogInformation(infoMessage);
                    }
                    else
                    {
                        // Validate semantically the json schema
                        logEntry = JsonLogEntry.DeserializeJsonLogEntry(messageBody, log);
                        var (validJson, errorValidation) = JsonLogEntry.ValidateJsonLogEntry(messageBody, log);
                        //Validate Trigram - compare entry from the json log with the trigram azure table storage 
                        (validTrigram, webhookUrl) = Trigram.ValidateTrigram(storageConnectionString, logEntry, trigramTableName, log);

                        if (validJson)
                        {
                            infoMessage = $"Task Run: Json Schema valid: {messageBody}";
                            log?.LogInformation(infoMessage);
                            if (validTrigram)
                            {
                                //Insert into ElasticSearch Cluster
                                var elasticLowLevelClient = emp_elastic_operations.ElasticConnect(elasticSearchClusterURI, elasticSearchAPIId, elasticSearchAPIKey, log);
                                await emp_elastic_operations.ElasticPutAsync(elasticLowLevelClient, logEntry, log);
                            }
                            else
                            {
                                // If not successful send to the Azure Storage  Teams queue 
                                log?.LogInformation($"Task Run: Application Trigram NOT valid: {messageBody}");
                                CloudQueue cloudQueue = AzureStorageQueueOperations.CreateAzureQueue(storageConnectionString, teamsQueueName, log);
                                var logQueue = new QueueLog() { ErrorMessage = $"Invalid trigram", LogEntry = logEntry, WebhookUrl = webhookUrl };
                                AzureStorageQueueOperations.InsertMessageQueue(cloudQueue, JsonConvert.SerializeObject(logQueue), log);
                            }
                        }
                        else
                        {
                            // If not valid  send to the Azure Storage Teams queue
                            infoMessage = $"Task Run: Json Schema NOT valid: {messageBody}";
                            log?.LogInformation(infoMessage);
                            CloudQueue cloudQueue = AzureStorageQueueOperations.CreateAzureQueue(storageConnectionString, teamsQueueName, log);
                            var logQueue = new QueueLog() { ErrorMessage = errorValidation, LogEntry = logEntry, WebhookUrl = webhookUrl };
                            AzureStorageQueueOperations.InsertMessageQueue(cloudQueue, JsonConvert.SerializeObject(logQueue), log);
                        }
                        await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
                    if (logEntry != null)
                    {
                        // send to the Azure Storage teams queue 
                        log?.LogInformation($"Task Run: exception raised {ex}");
                        CloudQueue cloudQueue = AzureStorageQueueOperations.CreateAzureQueue(storageConnectionString, teamsQueueName, log);
                        var logQueue = new QueueLog() { ErrorMessage = $"{ex}", LogEntry = logEntry, WebhookUrl = webhookUrl };
                        AzureStorageQueueOperations.InsertMessageQueue(cloudQueue, JsonConvert.SerializeObject(logQueue), log);

                        // send to the Azure Storage Error queue 
                        log?.LogInformation($"Task Run: exception raised {ex}");
                        cloudQueue = AzureStorageQueueOperations.CreateAzureQueue(storageConnectionString, errorQueueName, log);
                        AzureStorageQueueOperations.InsertMessageQueue(cloudQueue, messageBody, log);
                    }
                }
                
            }
            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.
            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
