using System;
using Microsoft.Extensions.Logging;

namespace ElasticTransformation
{
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Azure.Storage.Queue.Protocol;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;

    /// <summary>
    /// Class used to implement basic operations (Create/Insert) towards an Azure Storage Queue
    /// </summary>
    class AzureStorageQueueOperations
    {
        /// <summary>
        /// Method: CreateAzureQueue
        /// Goal: Create an Azure Storage Queue.
        /// </summary>
        /// <param name="storageConnectionString">The azure storage account connection string to access the azure storage account</param>
        /// <param name="errorQueueName">The error queue name</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>queue -> CloudQueue object</returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static CloudQueue CreateAzureQueue(string storageConnectionString, string errorQueueName, ILogger log)
        {
            try
            {
                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                // Retrieve a reference to a container.
                CloudQueue queue = queueClient.GetQueueReference(errorQueueName);
                if (queue.CreateIfNotExists())
                {
                    log?.LogInformation($"CreateAzureQueue:Created queue named: {errorQueueName}");
                    return queue;
                }
                else
                {
                    log?.LogInformation($"CreateAzureQueue:Queue already exists: {errorQueueName}");
                    return queue;
                }
            }
            catch (StorageException e)
            {
                log?.LogError($"CreateAzureQueue: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Method: InsertMessageQueue
        /// Goal: Inserts an entry in the Azure Storage Queue.
        /// </summary>
        /// <param name="queue">The azure storage account queue object</param>
        /// <param name="strMessage">Message to be inserted in the queue</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>queue -> CloudQueue object</returns>        
        /// <exception cref="StorageException"> Represents an exception thrown by the Azure Storage service</exception>
        public static void InsertMessageQueue(CloudQueue queue, string strMessage, ILogger log)
        {
            try
            {
                // Create a message and add it to the queue.
                CloudQueueMessage message = new CloudQueueMessage(strMessage);
                queue.AddMessage(message);
                log?.LogInformation($"InsertMessageQueue:Added message successfully: {strMessage}");

            }
            catch (StorageException e)
            {
                log?.LogError($"InsertMessageQueue: {e.Message}");
            }
        }

        /// <summary>
        /// Retrieve all elements in the queue
        /// </summary>
        /// <param name="queue">The queue</param>
        /// <param name="maxTime"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static List<string> ReadAllMessageQueue(CloudQueue queue, TimeSpan maxTime, ILogger log)
        {
            List<string> listQueue = new List<string>();
            CloudQueueMessage qm = queue.GetMessageAsync().GetAwaiter().GetResult();
            DateTime dtMax = DateTime.Now.Add(maxTime);
            while (qm != null)
            {
                listQueue.Add(qm.AsString);
                // We don't wait for this task to complete
                queue.DeleteMessageAsync(qm);
                qm = queue.GetMessageAsync().GetAwaiter().GetResult();
                if (dtMax < DateTime.Now)
                    break;
            };
            return listQueue;
        }

        public static int NumberOfElementsInQueue(CloudQueue queue, ILogger log)
        {
            queue.FetchAttributes();
            return queue.ApproximateMessageCount.GetValueOrDefault(0);
        }
    }
}
