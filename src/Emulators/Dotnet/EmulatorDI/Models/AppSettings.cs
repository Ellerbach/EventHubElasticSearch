namespace Appemulator.Models
{
    public class AppSettings
    {
        /// <summary>
        /// Sets header title in console window
        /// </summary>
        public string ConsoleTitle { get; set; }
        public string EmpConnectionString { get; set; }
        public string EmpEventHubName { get; set; }
        public string EmpSampleData { get; set; }
        
    }
}