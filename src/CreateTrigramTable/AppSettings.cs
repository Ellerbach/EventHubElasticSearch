using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

namespace AzureStorage
{
    /// <summary>
    /// Class used to reflect the local settings present on Settings.json
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The Storage Account connection string
        /// </summary> 
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// The CSV File Path to the trigram values
        /// </summary> 
        public string TrigramCSVFile { get; set; }

        /// <summary>
        /// The name of the Azure Storage table to be created and populated
        /// </summary> 
        public string TrigramTableName { get; set; }

        /// <summary>
        /// Method: LoadAppSettings
        /// Goal: Reads the settings present in Settings.json and saves them in a AppSettings memory structure
        /// </summary>
        /// <param name="jsonSettingsPath">The north star schema file path</param> 
        /// <returns>appSettings -> an AppSettings object</returns>        
        /// <exception cref="ConfigurationErrorsException">The exception that is thrown when any error occurs while configuration information is being read or written</exception>
        public static AppSettings LoadAppSettings(string jsonSettingsPath)
        {
            try
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(jsonSettingsPath)
                .AddJsonFile ("Settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
                
                IConfigurationRoot configRoot = configurationBuilder.Build();
                AppSettings appSettings = configRoot.Get<AppSettings>();
                return appSettings;
            }
            catch (ConfigurationErrorsException e)
            {
                 Console.WriteLine($"LoadAppSettings: ConfigurationErrorsException: {e.Message}");
                 throw;

            }
        }
    }
}
