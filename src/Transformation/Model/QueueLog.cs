using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticTransformation.Models
{
    /// <summary>
    /// This class is used specifically for posting in the queue the log errors
    /// </summary>
    public class QueueLog
    {
        /// <summary>
        /// The log entry which is faulty
        /// </summary>
        public JsonLogEntry LogEntry { get; set; }

        /// <summary>
        /// The webhook of the Teams channel to post the error
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// The error message to post on the Teams channel
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
