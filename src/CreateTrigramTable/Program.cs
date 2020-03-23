using AzureStorage.Model;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorage
{
    /// <summary>
    /// Program
    /// Goal is to temporarily implement the code to read from the local Trigram CSV file and update the Trigram Azure Storage Table
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point for the Program.cs 
        /// Executes asynchronously to wait for the operations in Azure
        /// </summary>
        /// <returns>void</returns>
        static async Task Main(string[] args)
        {
            //String to point out to the settings file location you are using
            string jsonSettingsPath = @"C:\PATHTOFILE_JSON_SETTING_FILE\";

            string storageConnectionString = AppSettings.LoadAppSettings(jsonSettingsPath).StorageConnectionString;
            string trigramCSVFile = AppSettings.LoadAppSettings(jsonSettingsPath).TrigramCSVFile;
            string tableName = AppSettings.LoadAppSettings(jsonSettingsPath).TrigramTableName;

            CloudTable table = await AzureTableStorageOperations.CreateTableAsync(storageConnectionString, tableName);

            //Create an instance of a trigram entity. See the Model\trigram.cs for a description of the entity.
            List<Trigram> trigramList = AzureTableStorageOperations.ReadFromCSVToTrigramList(trigramCSVFile);

            Trigram tg;
            // Insert the entity
            foreach (Trigram trigramItem in trigramList)
            {
                tg = await AzureTableStorageOperations.InsertOrMergeEntityAsync(table, trigramItem);
            }
        }
    }
}
