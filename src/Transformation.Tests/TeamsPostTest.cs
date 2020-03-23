using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace ElasticTransformation.Tests
{
    /// <summary>
    /// XUnit test for Team Post function
    /// </summary>
    public class TeamsPostTest
    {
        /// ///////////////////////////////////////////////
        // Test data
        /// ///////////////////////////////////////////////

        // Basic message used in unit tests - DO NOT CHANGE without changing string.replace in the test code
        const string BasicMessage = @"
            {
	        ""message"": ""Invalid Trigram"",
	        ""emp_log_entry"": {
		        ""trigram"": ""EFG"",
		        ""application"":""applicaiton name"",
		        ""layer"":""lifetest"",
		        ""level"":""DEBUG"",
		        ""date"":""Z2020etc"",
		        ""message"":""the original message"",
		        ""WebhookURL"":""""
                }
            }";

        /// <summary>
        /// Testing with a basic message - should pass
        /// </summary>
        [Fact(Skip = "false")]
        public async void TestRunWithSBasicMessage()
        {
            //TODO: Answer from real Teams URL should be mocked.

            string data = BasicMessage;
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.Equal(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        /// Testing with an empty message
        /// </summary>
        [Fact]
        public async void TestRunWithSNoMessage()
        {
            string data = "";
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        /// Testing with undeserializable message
        /// </summary>
        [Fact]
        public async void TestRunWithUnDeSerializableMessage()
        {
            string data = "This message is not deserializable...";
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }


        /// <summary>
        /// Testing with faulty trigram
        /// </summary>
        [Fact]
        public async void TestRunWithNoTrigram()
        {
            string data = BasicMessage;
            data.Replace(@"""trigram"": ""EFG""", @"""trigram"": ""EFGBARTRIGRAM""");
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        /// Testing with no WebHookURL in the environment variable
        /// </summary>
        [Fact]
        public async void TestRunWithNoWebHookInEnvVariable()
        {
            const string DefaultWebhookUrlEnvironmenent = "WebhookURL";

            // Saving current environment variable and setting its value to null (deletes it)
            string WebhookURL = Environment.GetEnvironmentVariable(DefaultWebhookUrlEnvironmenent);
            Environment.SetEnvironmentVariable(DefaultWebhookUrlEnvironmenent, null);

            string data = BasicMessage;
            var result = await TeamsNotification.RunFromDataString(data, null);

            // Restoring environment variable so that the next tests can be run
            Environment.SetEnvironmentVariable(DefaultWebhookUrlEnvironmenent, WebhookURL);

            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        /// Testing with non matching WebHokkURL
        /// </summary>
        [Fact]
        public async void TestRunWithNoMatchingWebHook()
        {
            string data = BasicMessage;
            data.Replace(@"""WebhookURL"": """"", @"""WebhookURL"": ""Bad Webhook URL""");
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        ///Test WebHookURL not responding (404 error)
        /// </summary>
        [Fact]
        public async void TestRunWithWebHookNotExisting()
        {
            const string DefaultWebhookUrlEnvironmenent = "WebhookURL";
            Environment.SetEnvironmentVariable(DefaultWebhookUrlEnvironmenent, "http://www.yahoo.frere");

            string data = BasicMessage;
            data.Replace(@"""WebhookURL"": """"", @"""WebhookURL"": ""http://www.yahoo.frere""");
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }

        /// <summary>
        ///Test WebHookURL error response (non 404 error)
        /// </summary>
        [Fact]
        public async void TestRunWithWebHookNotResponding()
        {
            const string DefaultWebhookUrlEnvironmenent = "WebhookURL";
            Environment.SetEnvironmentVariable(DefaultWebhookUrlEnvironmenent, "http://www.caf.fr");

            string data = BasicMessage;
            data.Replace(@"""WebhookURL"": """"", @"""WebhookURL"": ""http://www.caf.fr""");
            var result = await TeamsNotification.RunFromDataString(data, null);
            Assert.NotEqual(typeof(OkObjectResult), result.GetType());
        }
    }
}
