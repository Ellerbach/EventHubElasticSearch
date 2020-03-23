using System;
using Microsoft.Extensions.Logging;
using ElasticTransformation.Models;

namespace ElasticTransformation
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Class used to implement basic operations towards an Azure Storage Table:
    ///     1.Create Storage Account
    ///     2.Retrieve Table Object
    ///     3.Querying the Table) 
    /// </summary>
    public class emp_azure_storage_table_operations
    {
        /// <summary>
        /// Method: CreateStorageAccountFromConnectionString
        /// Goal: Retrieve azure storage account object.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>CloudStorageAccount object</returns>        
        /// <exception cref="FormatException">Invalid storage account information provided(application)</exception>
        /// <exception cref="ArgumentException">Invalid storage account information provided(sample)</exception>
        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString, ILogger log)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                log?.LogError("CreateStorageAccountFromConnectionString : FormatException : Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the configuration - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                log?.LogError("CreateStorageAccountFromConnectionString: ArgumentException : Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the configuration - then restart the sample.");
                throw;
            }
            return storageAccount;
        }

        /// <summary>
        /// Method: RetrieveTableObject
        /// Goal: Retrieve azure table object.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <param name="tableName">The azure trigram table name</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>CloudTable object</returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static CloudTable RetrieveTableObject(string storageConnectionString, string tableName, ILogger log)
        {
            try
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString, log);

                // Create a table client for interacting with the table service
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

                // Create a table client for interacting with the table service 
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                return cloudTable;
            }
            catch (StorageException e)
            {
                log?.LogError($"RetrieveTableObject: StorageException : {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Method: RetrieveEntityUsingPointQueryAsync
        /// Goal: Retrieve trigram entry from Azure table storage.
        /// </summary>
        /// <param name="table">The azure storage table to query
        /// <param name="partitionKey">The partition key to query the table</param>
        /// <param name="rowKey">The row key to query the table</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>Task<emp_trigram></returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static async Task<Trigram> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey, ILogger log)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<Trigram>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                Trigram trigram = result.Result as Trigram;
                string infoMessage;
                if (trigram != null)
                {
                    infoMessage = $"Triagram Exists: {trigram.RowKey}";
                    log?.LogInformation(infoMessage);
                }
                return trigram;
            }
            catch (StorageException e)
            {
                log?.LogError($"RetrieveEntityUsingPointQueryAsync: StorageException : {e.Message}");
                throw;
            }
        }
    }
}
