namespace Appemulator.Models
{
    /// <summary>
    /// Model class for EMP data
    /// </summary>
    public class LogEntry
    {
        public string trigram { get; set; }
        public string application { get; set; }
        public string layer { get; set; }
        public string level { get; set; }
        public string date { get; set; }
        public string message { get; set; }
        public string assembly_name { get; set; }
        public string assembly_version { get; set; }
        public string callsite { get; set; }
        public string url { get; set; }
        public string machinename { get; set; }
        public string aspnet_user_identity { get; set; }
        public string aspnet_sessionid { get; set; }
        public string exception { get; set; }
    }
}