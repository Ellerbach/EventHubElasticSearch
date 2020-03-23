using AzureStorage.Model;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureStorage
{
    /// <summary>
    /// Class used to implement basic operations towards an Azure Storage Table:
    ///     1.Create Storage Account
    ///     2.Create Table in the Storage account
    ///     3.Insert in the Table just created) 
    /// And a supporting function to read from the csv file to a trigram list
    /// </summary>
    public class AzureTableStorageOperations
    {

        /// <summary>
        /// Method: CreateStorageAccountFromConnectionString
        /// Goal: Retrieve azure storage account object.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <returns>CloudStorageAccount object</returns>        
        /// <exception cref="FormatException">Invalid storage account information provided(application)</exception>
        /// <exception cref="ArgumentException">Invalid storage account information provided(sample)</exception>
        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("CreateStorageAccountFromConnectionString : FormatException : Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("CreateStorageAccountFromConnectionString : ArgumentException : Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            return storageAccount;
        }

        /// <summary>
        /// Method: CreateTableAsync
        /// Goal: Create an Azure Storage table if unexisting.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <param name="tableName">The azure table name to be created</param>
        /// <returns>CloudStorageAccount object</returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static async Task<CloudTable> CreateTableAsync(string storageConnectionString, string tableName)
        {
            try
            {
                // Retrieve storage account information from connection string.
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);
                // Create a table client for interacting with the table service
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
                Console.WriteLine("CreateTableAsync: Create a temporary trigram table");
                // Create a table client for interacting with the table service 
                CloudTable table = tableClient.GetTableReference(tableName);
                if (await table.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("CreateTableAsync: Created Table named: {0}", tableName);
                }
                else
                {
                    Console.WriteLine("CreateTableAsync: Table {0} already exists", tableName);
                }
                Console.WriteLine();
                return table;
            }
            catch (StorageException e)
            {
                Console.WriteLine($"CreateTableAsync : StorageException : {e.Message}");
                Console.ReadLine();
                throw;
            }
        }

        /// <summary>
        /// Method: ReadFromCSV
        /// Goal: Supporting function that is able to read a csv file line by line and adds it to the Trigram memory list object
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <param name="tableName">The azure table name to be created</param>
        /// <returns>trigramList -> a list of Model.Trigram objects</returns>        
        /// <exception cref="IOException"> Represents an exception thrown by IO system</exception>
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static List<Trigram> ReadFromCSVToTrigramList(string csvPath)
        {
            try
            {
                string[] lines = File.ReadAllLines(csvPath);
                var trigramList = new List<AzureStorage.Model.Trigram>();
                int lineNumber = 0;
                foreach (string line in lines)
                {
                    //Ignoring the first header line
                    if (lineNumber == 0)
                    {
                        lineNumber++;
                        continue;
                    }
                    string[] columns = line.Split(',');
                    var trigram = new Trigram(columns[0], columns[1], columns[2]);
                    trigramList.Add(trigram);
                }
                return trigramList;
            }
            catch (IOException e)
            {
                Console.WriteLine($"ReadFromCSV : IOException : {e.Message}");
                Console.ReadLine();
                throw;
            }
            catch (StorageException e)
            {
                Console.WriteLine($"ReadFromCSV : StorageException : {e.Message}");
                Console.ReadLine();
                throw;
            }
        }

        /// <summary>
        /// Method: InsertOrMergeEntityAsync
        /// Goal: Insert a trigram object into an Azure table account
        /// </summary>
        /// <param name="table">The azure storage table object</param>
        /// <param name="entity">The trigram object to be inserted</param>
        /// <returns>insertedTrigram -> the inserted trigram object</returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static async Task<Trigram> InsertOrMergeEntityAsync(CloudTable table, Trigram entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Trigram");
            }
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                Trigram insertedTrigram = result.Result as Trigram;
                return insertedTrigram;
            }
            catch (StorageException e)
            {
                Console.WriteLine($"InsertOrMergeEntityAsync : StorageException : {e.Message}");
                Console.ReadLine();
                throw;
            }
        }
    }
}
