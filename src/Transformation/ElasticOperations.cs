using System;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ElasticTransformation.Models;

namespace ElasticTransformation
{
    /// <summary>
    /// Class used to implement basic operations towards the ElasticSearch Cluster:
    ///     1.ElasticConnect - to connect to the cluster
    ///     2.ElasticPutAsync - to send data to the cluster (POST method)
    /// </summary>
    public class emp_elastic_operations
    {
        /// <summary>
        /// Method: ElasticConnect
        /// Goal: Connects to the Elastic Cluster by using the Low Level Client package
        /// </summary>
        /// <param name="elasticURI">The ElasticSearch service uri</param> 
        /// <param name="apiId">The generated API ID to connect to the ElasticSearch cluster</param>
        /// <param name="apiKey">The generated API KEY to connect to the ElasticSearch cluster<</param> 
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns>The lowlevelClient object</returns>        
        /// <exception cref="UnexpectedElasticsearchClientException">Unknown exceptions, for instance a response from Elasticsearch not properly deserialized. These are sometimes bugs and should be reported.</exception>
        /// <exception cref="ElasticsearchClientException">
        ///             Known exceptions that includes: max retries or timeout reached, bad authentication, etc.
        ///             Elasticsearch itself returned an error (could not parse the request, bad query, missing field, etc.
        ///</exception>
        /// <exception cref="ElasticsearchClientException"></exception>
        public static ElasticLowLevelClient ElasticConnect(string elasticURI, string apiId, string apiKey, ILogger log)
        {
            try
            {
                var settings = new ConnectionConfiguration(new Uri(elasticURI))
                    .ApiKeyAuthentication(apiId, apiKey)
                    .RequestTimeout(TimeSpan.FromMinutes(2))
                    .ThrowExceptions();
                var lowlevelClient = new ElasticLowLevelClient(settings);
                log?.LogInformation($"ElasticConnect: Connected to Elastic Cluster successfully{elasticURI}");
                return lowlevelClient;
            }
            catch (UnexpectedElasticsearchClientException e)
            {
                log?.LogError($"ElasticConnect: UnexpectedElasticsearchClientException : Can't connect to Elastic Cluster{e.Message}");
                throw;
            }
            catch (ElasticsearchClientException e)
            {
                log?.LogError($"ElasticConnect: ElasticsearchClientException : Can't connect to Elastic Cluster{e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Method: ElasticPut
        /// Goal: Sends data to the ElasticSearch Cluster
        /// </summary>
        /// <param name="lowlevelClient">The ElasticSearch LowLevelClient Object, returned after the connection is made</param> 
        /// <param name="jsonLogEntry">The emp_json_log_entry object entry with the representation of the log message</param>
        /// <param name="log">The ILogger object to log information and errors</param>
        /// <returns></returns>        
        /// <exception cref="UnexpectedElasticsearchClientException">Unknown exceptions, for instance a response from Elasticsearch not properly deserialized. These are sometimes bugs and should be reported.</exception>
        /// <exception cref="ElasticsearchClientException">
        ///             Known exceptions that includes: max retries or timeout reached, bad authentication, etc.
        ///             Elasticsearch itself returned an error (could not parse the request, bad query, missing field, etc.
        ///</exception>
        public static async Task ElasticPutAsync(ElasticLowLevelClient lowlevelClient, JsonLogEntry jsonLogEntry, ILogger log)
        {
            try
            {
                string indexString;
                DateTime dt = Convert.ToDateTime(jsonLogEntry.Date);
                string strYear = (dt.Year.ToString());
                indexString = $"apps_{jsonLogEntry.Trigram.ToLower()}_{strYear}";
                var asyncIndexResponse = await lowlevelClient.IndexAsync<StringResponse>(indexString, PostData.Serializable(jsonLogEntry));
                string indexResponse = asyncIndexResponse.Body;
                var success = asyncIndexResponse.Success;
                if (success)
                {
                    log?.LogInformation($"ElasticPut: Log entry successfully sent to Elastic Cluster {asyncIndexResponse.Body}");
                }
                else
                {
                    log?.LogInformation($"ElasticPut: An exception was thrown by the Elastic method {asyncIndexResponse.OriginalException}");
                    throw new Exception("Could not write in elastic");
                }
            }
            catch (UnexpectedElasticsearchClientException e)
            {
                log?.LogError($"ElasticPut: UnexpectedElasticsearchClientException : {e.Message}");
                throw e;
            }
            catch (ElasticsearchClientException e)
            {
                log?.LogError($"ElasticPut: ElasticsearchClientException : {e.Message}");
                throw e;
            }
        }

        
    }
}