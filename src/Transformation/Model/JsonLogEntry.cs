namespace ElasticTransformation.Models
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Text.RegularExpressions;
    using System.Globalization;

    /// <summary>
    /// Class to handle the mandatory fields in a log
    /// </summary>
    public class JsonLogEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonLogEntry()
        { }

        /// <summary>
        /// Trigram: represents a 3 char string that uniquely identifies the application. e.g.: "arc"
        /// </summary>
        [JsonProperty("trigram")]
        public string Trigram { get; set; }

        /// <summary>
        /// Level, E.G.: "DEBUG", must be capital letter
        /// </summary>
        [JsonProperty("level")]
        public string Level { get; set; }

        /// <summary>
        /// Date, e.g.: "2020-02-11T17:38:32.0312581Z", ISO8610 format, with year, month, day, hour, minute and seconds mandatory, fraction of seconds allowed, finishing by Z with no timezone
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }

        /// <summary>
        /// The message itself, e.g.: super message with a great description of what is happening
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Method: ValidateJsonLogEntry
        /// Goal: Validates a json string entry against the json north star schema
        /// </summary>
        /// <param name="jsonFromEventHubMessage">The json string event coming from the eventhub</param> 
        /// <param name="log">The ILogger object to log information and errors</param> 
        /// <returns>valid -> boolean that reflects if the json entry is valid = true or if the json entry is not valid = false</returns>        
        /// <exception cref="IOException">The exception that is thrown when an I/O error occurs</exception>
        /// <exception cref="Newtonsoft.Json.Schema.JSchemaException">The exception thrown when an error occurs in Json.NET Schema</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">Not documented</exception>
        public static (Boolean isValid, string errorMessage) ValidateJsonLogEntry(string jsonFromEventHubMessage, ILogger log)
        {
            string errorMessage = "";
            bool isValid = true;
            try
            {
                // Deserialize the message
                var emp = JsonConvert.DeserializeObject<JsonLogEntry>(jsonFromEventHubMessage);                

                // Check if we have all the fields
                if (string.IsNullOrEmpty(emp.Date))
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Date)} field is null or empty. ";
                    isValid = false;
                }

                if (string.IsNullOrEmpty(emp.Trigram))
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Trigram)} field is null or empty. ";
                    isValid = false;
                }

                if (string.IsNullOrEmpty(emp.Message))
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Message)} field is null or empty. ";
                    isValid = false;
                }

                if (string.IsNullOrEmpty(emp.Level))
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Level)} field is null or empty. ";
                    isValid = false;
                }

                // level must be upper case
                string regLevelValidation = "[^AZ]";
                var match = Regex.Match(emp.Level, regLevelValidation);
                if (!match.Success)
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Level)} field is not uppercase. ";
                    isValid = false;
                }

                // Trigram must be 3 character long
                if (emp.Trigram.Length != 3)
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Trigram)} field is not 3 character long. ";
                    isValid = false;
                }

                // Validate iso 8601 date
                // "2020-02-11T17:38:32.0312581Z", ISO8610 format, with year, month, day, hour, minute and seconds mandatory, fraction of seconds allowed, finishing by Z with no timezone
                if (!IsValidIso8601Date(emp.Date))
                {
                    errorMessage += $"ValidateJsonLogEntry: {nameof(Date)} field is not ISO8601 with year, month, day, hour, minute, seconds mandatory, milliseconds optional and no timezone. ";
                    isValid = false;
                }

                if (!isValid)
                    log?.LogInformation(errorMessage);

                return (isValid, errorMessage);
            }
            catch (Exception e)
            {
                log?.LogError($"ValidateJsonLogEntry: Exception: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if a date is a valid one, should be "2020-02-11T17:38:32.0312581Z", ISO8610 format, 
        /// with year, month, day, hour, minute and seconds mandatory, 
        /// fraction of seconds allowed, finishing by Z with no timezone
        /// </summary>
        /// <param name="dateToValidate"></param>
        /// <returns></returns>
        public static bool IsValidIso8601Date(string dateToValidate)
        {
            string[] validFormats = new string[]
            {
                @"yyyy-MM-dd\THH:mm:ss\Z",
                @"yyyy-MM-dd\THH:mm:ss.f\Z",
                @"yyyy-MM-dd\THH:mm:ss.ff\Z",
                @"yyyy-MM-dd\THH:mm:ss.fff\Z",
                @"yyyy-MM-dd\THH:mm:ss.ffff\Z",
                @"yyyy-MM-dd\THH:mm:ss.fffff\Z",
                @"yyyy-MM-dd\THH:mm:ss.ffffff\Z",
                @"yyyy-MM-dd\THH:mm:ss.fffffff\Z",
            };

            DateTime dateChecked;
            return DateTime.TryParseExact(dateToValidate, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateChecked);
        }

        /// <summary>
        /// Method: DeserializeJsonLogEntry
        /// Goal: Converts a json string event coming from EventHub to a emp_json_log_entry object.
        /// </summary>
        /// <param name="jsonFromEventHubMessage">The json string event coming from the eventhub</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>logEntry -> emp_json_log_entry object</returns>        
        /// <exception cref="JsonException"> </exception>
        /// When <paramref name="jsonFromEventHubMessage"/> is invalid  OR
        /// When <paramref returnType="emp_json_log_entry"/> not compatible with the json OR
        /// When <paramref name="jsonFromEventHubMessage"/> There is remaining data in the span beyond a single JSON value
        /// <exception cref="ArgumentNullException"> </exception>
        /// When <paramref name="jsonFromEventHubMessage"/> is null
        /// </exception>
        public static JsonLogEntry DeserializeJsonLogEntry(string jsonFromEventHubMessage, ILogger log)
        {
            try
            {
                JsonLogEntry logEntry = JsonConvert.DeserializeObject<JsonLogEntry>(jsonFromEventHubMessage);
                log?.LogInformation($"DeserializeJsonLogEntry: Json was deserialized successfully: {jsonFromEventHubMessage}");
                return logEntry;
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                log?.LogError($"DeserializeJsonLogEntry: JsonException: {e.Message}");
                throw;
            }
            catch (ArgumentNullException e)
            {
                log?.LogError($"DeserializeJsonLogEntry: ArgumentNullException: {e.Message}");
                throw;
            }
        }
    }
}