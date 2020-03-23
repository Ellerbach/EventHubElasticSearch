using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ElasticTransformation.Models
{    
    /// <summary>
    /// Class used to validate that the trigram is correct
    /// </summary>
    public class Trigram : TableEntity
    {
        /// <summary>
        /// Name of the application, complete name of the System
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents a 3 char string that uniquely identifies the application. Eg: "arc"
        /// </summary>
        public string ApplicationTrigram { get; set; }

        /// <summary>
        /// The Teams webhook of the dev team owning the application 
        /// </summary>
        public string WebHook { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Trigram()
        { }

        /// <summary>
        /// Constructor of the table entity: emp_trigram
        /// Goal: builds emp_trigram object based on the 3 received arguments. 
        /// Additionally it defines the partition key and row key to read from the azure storage table
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="appTrigram">The application trigram</param>
        /// <param name="webHook">WebHook url</param>
        /// <returns>emp_trigram object</returns>        
        public Trigram(string name, string appTrigram, string webHook)
        {
            //properties
            Name = name;
            ApplicationTrigram = appTrigram;
            WebHook = webHook;
            //table keys for azure table storage
            PartitionKey = ApplicationTrigram;
            RowKey = ApplicationTrigram;
        }

        /// <summary>
        /// Method: ValidateTrigram
        /// Goal: Validates if a trigram entry from the log is present in the emp-trigram storage table (single source of true) in Azure.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure trigram table</param>
        /// <param name="logEntry">The log entry</param>
        /// <param name="trigramTableName">The azure trigram table name</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>validTrigram -> a boolean that reflects if the trigram is valid = true or if the trigram is not valid = false and the webhook url</returns>        
        /// <exception cref="ArgumentException">The exception that is thrown when one of the arguments provided to a method is not valid</exception>
        public static (Boolean isValid, string webhookUrl) ValidateTrigram(string storageConnectionString, JsonLogEntry logEntry, string trigramTableName, ILogger log)
        {
            try
            {
                //Get the json string from the EventHub and converts to upper case - as Triagram table is stored as upper case
                string triagramFromLogEntry = logEntry.Trigram.ToUpper();
                //Retrieve Azure Cloud Table Entity that holds the information for the Application Trigrams
                CloudTable cloudTable = emp_azure_storage_table_operations.RetrieveTableObject(storageConnectionString, trigramTableName, log);
                //Query Azure Cloud Table with Partition and Row Key
                Task<Trigram> triagramFromAzureTable = emp_azure_storage_table_operations.RetrieveEntityUsingPointQueryAsync(cloudTable, triagramFromLogEntry, triagramFromLogEntry, log);
                if (triagramFromAzureTable.Result is null)
                {
                    //Trigram not present in the Azure Storage Trigram Table
                    log?.LogInformation($"ValidateTrigram: Trigram entry doesn't exist on the Trigram table: {triagramFromLogEntry}");
                    return (false, null);
                }
                else
                {
                    //Compare Trigram in the Log entry with the Azure Trigram table
                    //If equal return true, if not returns false
                    string stringTrigramFromAzureTable = triagramFromAzureTable.Result.ApplicationTrigram;
                    bool validTrigram = String.Equals(stringTrigramFromAzureTable, triagramFromLogEntry);
                    log?.LogInformation($"ValidateTrigram: Trigram entry is valid: {triagramFromLogEntry}");
                    return (validTrigram, triagramFromAzureTable.Result.WebHook);
                }
            }
            catch (ArgumentException e)
            {
                log?.LogError($"ValidateTrigram: Exception: {e.Message}");
                throw;
            }
        }
    }
}
