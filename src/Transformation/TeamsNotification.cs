using ElasticTransformation.Models;
using MessageCardModel;
using MessageCardModel.Actions;
using MessageCardModel.Actions.OpenUri;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ElasticTransformation
{
    /// <summary>
    /// Class to post a message in Teams using a Webhook, implementation into a function
    /// </summary>
    public static class TeamsNotification
    {
        const string DefaultWebhookUrlEnvironmenent = "EMP_WEBHOOK_TEAMS_URL";
        const string DefaultKibanaUrl = "EMP_KIBANA_URL";

        /// <summary>
        /// Function callback to post a message in Teams.
        /// TODO: work on the "message" parameter, so far just a simple text mokup
        /// </summary>
        /// <remarks>
        /// The object posted should be like this:
        /// {
        ///     "message":"error test with objects",
        /// 	"emp_json_log_entry": {
        /// 	    "trigram": "abc",
        /// 	    "application": "Axaim.Membership.Admin",
        /// 	    "layer": "lifetest",
        /// 	    "level": "DEBUG",
        /// 	    "date": "Z2020etcetc",
        /// 	    "message": "the original message",
        /// 	    "WebhookURL": ""
        ///     }
        /// }
        /// TODO: adjust this based on the real emp_json_log_entry class
        /// </remarks>
        /// <param name="req">The http request</param>
        /// <param name="log">Logger to log</param>
        /// <returns>a result action wit OK if all goes right or BadRequestObjectResult in case of issue</returns>
        [FunctionName("PostTeamsMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return await RunFromDataString(requestBody, log);
        }

        /// <summary>
        /// Function callback to post a message in Teams.
        /// TODO: work on the "message" parameter, so far just a simple text mokup
        /// </summary>
        /// <remarks>
        /// The object posted should be like this:
        /// {
        ///     "message":"error test with objects",
        /// 	"emp_json_log_entry": {
        /// 	    "trigram": "abc",
        /// 	    "application": "Axaim.Membership.Admin",
        /// 	    "layer": "lifetest",
        /// 	    "level": "DEBUG",
        /// 	    "date": "Z2020etcetc",
        /// 	    "message": "the original message",
        /// 	    "WebhookURL": ""
        ///     }
        /// }
        /// TODO: adjust this based on the real emp_json_log_entry class
        /// </remarks>
        /// <param name="req">The http request</param>
        /// <param name="log">Logger to log</param>
        /// <returns>a result action wit OK if all goes right or BadRequestObjectResult in case of issue</returns>
        public static async Task<IActionResult> RunFromDataString(string requestBody, ILogger log)
        {
            // Get the message from the body
            string message;
            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(requestBody);
                message = data?.message;
            }
            catch (JsonReaderException ex)
            {
                log?.LogError(ex, "Error deserializing the message");
                return (ActionResult)new BadRequestObjectResult($"Error: {ex}");
            }

            log?.LogInformation("Processing a request to post an error on a Teams channel");
            if (message == null)
            {
                log?.LogError("No error message provided");
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }

            // Get the emp_json_log_entry from the body   
            JsonLogEntry logEntry = new JsonLogEntry();
            logEntry.Trigram = data?.emp_log_entry?.trigram;
            logEntry.Level = data?.emp_log_entry?.level;
            logEntry.Date = data?.emp_log_entry?.date;
            logEntry.Message = data?.emp_log_entry?.message;
            string webhookURL = data?.WebhookURL;

            return await PostOnTeams(message, webhookURL, logEntry, log);
        }

        /// <summary>
        /// Post a message in Teams.
        /// </summary>
        /// <param name="message">A message</param>
        /// <param name="webhookURL">The webhook URL</param>
        /// <param name="logEntry">A log entry</param>
        /// <param name="log">The logger</param>
        /// <returns>>a result action wit OK if all goes right or BadRequestObjectResult in case of issue</returns>
        public static async Task<IActionResult> PostOnTeams(string message, string webhookURL, JsonLogEntry logEntry, ILogger log)
        {
            string url = string.IsNullOrEmpty(webhookURL) ? Environment.GetEnvironmentVariable(DefaultWebhookUrlEnvironmenent) : webhookURL;

            if (string.IsNullOrEmpty(url))
            {
                log?.LogError($"No default {DefaultWebhookUrlEnvironmenent} environment variable provided");
                return new BadRequestObjectResult($"A default WebhookURL needs to be provided");
            }

            MessageCard card;
            // Prepare the message
            if (logEntry != null)
            {
                card = CreateMessageCard(message, logEntry);
            }
            else
            {
                card = CreateBasicMessage(message);
            }

            var json = JsonConvert.SerializeObject(card);

            return await PostOnTeamsMessage(json, url, log);
        }

        /// <summary>
        /// Post a message on teams where the card or the text is directly given
        /// </summary>
        /// <param name="message"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<IActionResult> PostOnTeamsMessage(string message, string url, ILogger log)
        {
            // Post the message
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                var postData = message;
                var dataToSend = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = dataToSend.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(dataToSend, 0, dataToSend.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    log?.LogError($"Error: HTTP status code " + response.StatusCode + " from " + url, "Error posting a the message on Team thru the Webhook");
                    return (ActionResult)new BadRequestObjectResult($"Error: HTTP status code " + response.StatusCode + " from " + url);
                }

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                log?.LogInformation("Message posted on Teams thru Webhook successfully");

                // Succes for posting in Teams webhook returns string "1"
                if (responseString == "1")
                {
                    return (ActionResult)new OkObjectResult($"Error message posted correctly: {message}, response: {responseString}");
                }
                else
                {
                    log?.LogError($"Error: bad response from teams Post Service: " + responseString, "Error posting a the message on Team thru the Webhook");
                    return (ActionResult)new BadRequestObjectResult($"Error: bad response from teams Post Service: " + responseString);
                }
            }
            catch (WebException ex)
            {
                log?.LogError(ex, $"Error posting a the message on Team thru the Webhook: {url}, card: {message} ");
                return (ActionResult)new BadRequestObjectResult($"Error: {ex}");
            }
        }

        /// <summary>
        /// Create a simple card message when there is no way to handle the log entry
        /// </summary>
        /// <param name="message">The message to post</param>
        /// <returns>A message card ready to post</returns>
        public static MessageCard CreateBasicMessage(string message)
        {
            MessageCard card = new MessageCard();
            card.Title = "Unknown log entry";
            card.Text = message;

            return card;
        }

        /// <summary>
        /// Create a message card ready to publish in Teams
        /// TODO: rather than string, need to create a class or have multiple parameters to the message to post
        /// </summary>
        /// <param name="message">The message to post see TODO</param>
        /// <returns>A message card ready to post</returns>
        public static MessageCard CreateMessageCard(string message, JsonLogEntry logEntry)
        {
            MessageCard card = new MessageCard();

            card.Title = "Log entry error";
            card.Text = $"Error: {message}";

            card.Sections = new List<Section>
            {
                new Section
                {
                    ActivityTitle = logEntry?.Trigram,
                    ActivitySubtitle = DateTime.Now.ToString(),
                    Facts = new List<Fact>
                    {
                        new Fact{ Name = $"{nameof(logEntry.Date)}", Value = logEntry?.Date },
                        new Fact{ Name = $"{nameof(logEntry.Level)}", Value = logEntry?.Level }
                    },
                    Text = $"Original message: {logEntry?.Message}"
                }
            };

            card.Actions = new List<IAction>
            {
                new OpenUriAction
                {
                    Type = ActionType.OpenUri,
                    Name = "View on site",
                    Targets = new List<Target>{ new Target { OS = TargetOs.Default, Uri = string.Format(Environment.GetEnvironmentVariable(DefaultKibanaUrl), logEntry?.Trigram) } }
                }
            };

            return card;
        }

        /// <summary>
        /// Create a card with multiple log entries
        /// </summary>
        /// <param name="webHookURL">The webhook url</param>
        /// <param name="logEntries">The entries with the error message</param>
        /// <returns>A message card</returns>
        public static MessageCard CreateMessageCardFromList(string webHookURL, List<QueueLog> logEntries)
        {
            const int MaxMessages = 3;
            MessageCard card = new MessageCard();

            card.Title = "Log entry error";            

            // Group the trigrams
            var logTrigrams = logEntries.Select(m => m.LogEntry.Trigram).Distinct();
            card.Text = $"Number of messages: {logEntries.Count}, {logTrigrams.Count()} trigram impacted";

            var sections = new List<Section>();
            var actions = new List<IAction>();
            foreach (var trigram in logTrigrams)
            {
                var sec = new Section
                {
                    ActivityTitle = trigram,
                    ActivitySubtitle = DateTime.Now.ToString(),
                };

                // Select only entries for the specific trigram
                var logEntriesTrigram = logEntries.Where(m => m.LogEntry.Trigram == trigram);
                sec.Text = $"Number of errors: {logEntriesTrigram.Count()} ({MaxMessages} max displayed)";

                int messagesPosted = 1;
                var facts = new List<Fact>();
                foreach (var log in logEntriesTrigram)
                {
                    var logEntry = log.LogEntry;

                    List<Fact> fctsLog;
                    if (trigram != TeamsQueueNotification.NoTrigram)
                    {
                        fctsLog = new List<Fact>
                        {
                            new Fact { Name = $"_____", Value =""},
                            new Fact { Name = $"{nameof(logEntry.Date)}", Value = logEntry?.Date },
                            new Fact { Name = $"Original message:", Value = logEntry?.Message },
                            new Fact { Name = $"Error message:", Value = log.ErrorMessage }
                        };
                    }
                    else
                    {
                        fctsLog = new List<Fact>
                        {
                            new Fact { Name = $"_____", Value =""},
                            new Fact { Name = $"Error message:", Value = log.ErrorMessage }
                        };
                    }
                    facts.AddRange(fctsLog);
                    if (messagesPosted++ >= MaxMessages)
                        break;
                };
                sec.Facts = facts;
                sections.Add(sec);
                card.Sections = sections;
                if (trigram != TeamsQueueNotification.NoTrigram)
                {
                    var action =

                        new OpenUriAction
                        {
                            Type = ActionType.OpenUri,
                            Name = $"View {trigram} on Kibana",
                            Targets = new List<Target> { new Target { OS = TargetOs.Default, Uri = string.Format(Environment.GetEnvironmentVariable(DefaultKibanaUrl), trigram) } }
                        };

                    actions.Add(action);
                }
            }
            card.Actions = actions;
            return card;
        }
    }
}
