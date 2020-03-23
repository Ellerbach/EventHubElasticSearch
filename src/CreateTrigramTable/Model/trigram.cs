using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace AzureStorage.Model
{
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
        /// Constructor of the table entity: Trigram
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
    }
}
