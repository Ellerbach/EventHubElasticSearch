
using ElasticTransformation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticTransformation
{
    public class TeamsQueueNotification
    {
        public const string NoTrigram = "No trigram";
        private const string DefaultWebhookUrlEnvironmenent = "EMP_WEBHOOK_TEAMS_URL";

        /// <summary>
        /// To be implemented
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <param name="executionContext"></param>
        [FunctionName("TimerTriggerPostTeams")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext executionContext)
        {
            // Create a time span to retreive the elements in the queue
            TimeSpan maxTimeSpanRetreive = TimeSpan.FromSeconds(50);

            string jsonSettingsPath = executionContext.FunctionAppDirectory;

            //App Settings from local.settings.json
            string storageConnectionString = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_STORAGE_ACCOUNT_CONNECTION_STRING;
            string teamsQueueName = ApSettings.LoadAppSettings(jsonSettingsPath, log).EMP_STORAGE_ACCOUNT_TEAMS_QUEUE_NAME;

            CloudQueue cloudQueue = AzureStorageQueueOperations.CreateAzureQueue(storageConnectionString, teamsQueueName, log);
            List<String> teamPostListStrings = AzureStorageQueueOperations.ReadAllMessageQueue(cloudQueue, maxTimeSpanRetreive, log);

            // Dictionnary (indexed by WebHookURL name) of Lists of emp_queue_log 
            Dictionary<string, List<QueueLog>> queueEntries = new Dictionary<string, List<QueueLog>>();
            foreach (string teamsPostString in teamPostListStrings)
            {
                try
                {
                    QueueLog emp;
                    emp = JsonConvert.DeserializeObject<QueueLog>(teamsPostString);

                    // check we have a webhook, if not, then use the default one
                    emp.WebhookUrl = string.IsNullOrEmpty(emp.WebhookUrl) ? Environment.GetEnvironmentVariable(DefaultWebhookUrlEnvironmenent) : emp.WebhookUrl;
                    // Test if there is a emp, if not create a standard one                   
                    emp.LogEntry = emp.LogEntry == null ? new JsonLogEntry() { Trigram = NoTrigram } : emp.LogEntry;
                    emp.LogEntry.Trigram = string.IsNullOrEmpty(emp.LogEntry.Trigram) ? NoTrigram : emp.LogEntry.Trigram;

                    // Creates the list for a WebHook if it does not exists yet
                    if (!queueEntries.ContainsKey(emp.WebhookUrl)) queueEntries.Add(emp.WebhookUrl, new List<QueueLog>());

                    //Add the log item in the appropriate queue for the Webhook
                    List<QueueLog> WebHookQueue = queueEntries[emp.WebhookUrl];
                    WebHookQueue.Add(emp);
                }
                catch (JsonException)
                {
                    // Ignore any wrong log
                    log?.LogError($"Queue deserialization not correct");
                }
            }

            // Looping on the list of queues and sending to teams
            foreach (KeyValuePair<string, List<QueueLog>> kvp in queueEntries)
            {
                PostLogQueueToTeams((string)kvp.Key, (List<QueueLog>)kvp.Value, log);
            }
        }

        /// <summary>
        /// Post a list of logs errors to the a specific Teams channel
        /// </summary>
        /// <param name="webHookURL">the webhook url</param>
        /// <param name="logEntries">The log entry list</param>
        /// <param name="log">The logger</param>
        static void PostLogQueueToTeams(string webHookURL, List<QueueLog> logEntries, ILogger log)
        {
            var card = TeamsNotification.CreateMessageCardFromList(webHookURL, logEntries);
            var res = TeamsNotification.PostOnTeamsMessage(JsonConvert.SerializeObject(card), webHookURL, log).GetAwaiter().GetResult();
            if (res.GetType() != typeof(OkObjectResult))
            {
                log?.LogError($"post not successful on Teams: {webHookURL}");
            }
        }
    }
}
