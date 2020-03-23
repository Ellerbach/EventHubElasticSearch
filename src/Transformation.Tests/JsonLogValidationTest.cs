using ElasticTransformation.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using Xunit;

namespace ElasticTransformation.Tests
{
    /// <summary>
    /// Unit tests for json serialization validation
    /// </summary>
    public class JsonLogValidationTest
    {
        /// <summary>
        /// Do not change any of those strings
        /// </summary>
        const string ValidMessage = @"{ 
                ""trigram"" : ""arc"",
                ""level"": ""DEBUG"", 
                ""date"": ""2020-02-11T17:38:36.9218638Z"",
                ""message"" : ""Life Test Requested"", 
                ""machinename"": ""FR2AS1WS3-1119"", 
                ""iis_site_name"": ""arc301"", 
                ""aspnet_user_identity"": ""AXA-M\\zu_mwp_arc"" 
            }";

        /// <summary>
        /// Do not change any of those strings
        /// </summary>
        const string NonValidMessage = @"{ 
                ""trigram"" : ""arc"",
                ""level"": ""DEBUG"", 
                ""date"": ""2020-02-11T17:38Z"",
                ""message"" : ""Life Test Requested"", 
                ""machinename"": ""FR2AS1WS3-1119"", 
                ""iis_site_name"": ""arc301"", 
                ""aspnet_user_identity"": ""AXA-M\\zu_mwp_arc"" 
            }";


        // Testing a simple valid message 
        [Fact]
        public void ValidateJsonWithMinimalRequirements()
        {
            var (emp, message) = JsonLogEntry.ValidateJsonLogEntry(ValidMessage, null);
            Assert.True(emp);
        }

        // Testing a simple invalid message 
        [Fact]
        public void FailJsonWithMinimalRequirements()
        {
            var (emp, message) = JsonLogEntry.ValidateJsonLogEntry(NonValidMessage, null);
            Assert.False(emp);
        }

        // Testing undeserilizable message
        [Fact]
        public void FailJsonWithUndeserializableMessage()
        {
            Assert.Throws<JsonReaderException>(() => JsonLogEntry.ValidateJsonLogEntry("Undeserializable message", null));
        }

        // Testing empty fields        
        [Fact]
        public void FailJsonWithEmptyMessageParts()
        {
            string message = ValidMessage;

            // testing date
            message = message.Replace(@"""date"": ""2020-02-11T17:38:36.9218638Z""", @"""date"": """"");
            var (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing trigram
            message = message.Replace(@"""trigram"" : ""arc""", @"""trigram"" : """"");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing trigram
            message = message.Replace(@"""trigram"" : ""arc""", @"""trigram"" : """"");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing message
            message = message.Replace(@"""message"" : ""Life Test Requested""", @"""message"" : """"");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing level
            message = message.Replace(@"""level"": ""DEBUG""", @"""level"": ""DEBUG""");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing level must be uppercase
            message = message.Replace(@"""level"": ""DEBUG""", @"""level"": ""DEbUG""");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);

            // testing length (must be 3)
            message = message.Replace(@"""trigram"" : ""arc""", @"""trigram"" : ""ABCD""");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);
            message = message.Replace(@"""trigram"" : ""arc""", @"""trigram"" : ""AD""");
            (emp, mess) = JsonLogEntry.ValidateJsonLogEntry(message, null);
            Assert.False(emp);
        }

        // Testing various date formats
        [Fact]
        public void ValidVariousDates()
        {
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.9218638Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.9Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.92Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.921Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.9218Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.92186Z"));
            Assert.True(JsonLogEntry.IsValidIso8601Date("2020-02-11T17:38:36.921863Z"));

            Assert.False(JsonLogEntry.IsValidIso8601Date(null));
            Assert.False(JsonLogEntry.IsValidIso8601Date(string.Empty));
            Assert.False(JsonLogEntry.IsValidIso8601Date("Non date format"));
            Assert.False(JsonLogEntry.IsValidIso8601Date("2020-02-11"));
            Assert.False(JsonLogEntry.IsValidIso8601Date("2020-02-11T11:15"));
            Assert.False(JsonLogEntry.IsValidIso8601Date("2020-02-11T11:15Z"));
        }


        // Testing a simple valid message deserialization
        [Fact]
        public void ValidateDeserializeJsonLogEntryWithMinimalRequirements()
        {
            JsonLogEntry emp = JsonLogEntry.DeserializeJsonLogEntry(ValidMessage, null);
            Assert.NotNull(emp);
        }

        // Testing a simple invalid message deserialization
        [Fact]
        public void ValidateDeserializeJsonLogEntryWithMinimalRequirementsFailure()
        {
            Assert.Throws<Newtonsoft.Json.JsonReaderException>(() => JsonLogEntry.DeserializeJsonLogEntry("Non deserialisable message", null));
        }

    }
}
