namespace ElasticTransformation
{
    using System.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Class used to reflect the local settings present on local.settings.json OR the Application Settings in the App Service in Azure
    /// </summary>
    public class ApSettings
    {
        /// <summary>
        /// The Azure Functions runtime uses this storage account connection string for all functions 
        /// </summary>
        public string AzureWebJobsStorage { get; set; }

        /// <summary>
        /// Version of the Azure Functions runtime (3) 
        /// </summary>
        public string FUNCTIONS_WORKER_RUNTIME { get; set; }

        /// <summary>
        /// Connection String to the Event Hub Namespace receiving the application logs 
        /// </summary>
        public string EMP_EVENTHUB_CONNECTION_STRING { get; set; }

        /// <summary>
        /// The name of the Hub receiving the application logs
        /// </summary>
        public string EMP_EVENT_HUB_NAME { get; set; }

        /// <summary>
        /// Connection string for the storage account that holds the Azure Table for the Trigrams and the Azure Queue Error
        /// </summary>
        public string EMP_STORAGE_ACCOUNT_CONNECTION_STRING { get; set; }

        /// <summary>
        /// Name of the Error Queue in Azure Storage
        /// </summary>
        public string EMP_STORAGE_ACCOUNT_ERROR_QUEUE_NAME { get; set; }

        /// <summary>
        /// Name of the Teams Queue in Azure Storage
        /// </summary>
        public string EMP_STORAGE_ACCOUNT_TEAMS_QUEUE_NAME { get; set; }
        
        /// <summary>
        /// Name of the Trigram table in Azure Storage
        /// </summary>        
        public string EMP_TRIGRAM_TABLE_NAME { get; set; }

        /// <summary>
        /// The uri of the ElasticSearch Cloud Service
        /// </summary>  
        public string EMP_ELASTIC_SEARCH_CLUSTER_URI { get; set; }

        /// <summary>
        /// The API Id secret to access the ElasticSearch Cloud Service
        /// </summary>    
        public string EMP_ELASTIC_SEARCH_API_ID { get; set; }

        /// <summary>
        /// The API Key secret to access the ElasticSearch Cloud Service
        /// </summary>  
        public string EMP_ELASTIC_SEARCH_API_KEY { get; set; }

        /// <summary>
        /// Method: LoadAppSettings
        /// Goal: Reads the settings present in local.settings.json and saves them in a emp_app_settings memory structure
        /// </summary>
        /// <param name="jsonSettingsPath">The north star schema file path</param> 
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>ap -> an emp_app_settings object</returns>        
        /// <exception cref="ConfigurationErrorsException">The exception that is thrown when any error occurs while configuration information is being read or written</exception>
        public static ApSettings LoadAppSettings(string jsonSettingsPath, ILogger log)
        {
            try
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(jsonSettingsPath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                IConfigurationRoot confRoot = configurationBuilder.Build();
                ApSettings ap = confRoot.Get<ApSettings>();
                return ap;
            }
            catch (ConfigurationErrorsException e)
            {
                log?.LogError($"LoadAppSettings: ConfigurationErrorsException: {e.Message}");
                throw;
            }
        }
    }
}