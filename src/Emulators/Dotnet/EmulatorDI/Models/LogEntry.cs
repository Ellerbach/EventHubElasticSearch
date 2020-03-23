namespace Appemulator.Models
{
    /// <summary>
    /// Model class for data
    /// </summary>
    public class LogEntry
  {
    public string trigram { get; set; }
    public string application { get; set; }
    public string layer { get; set; }
    public string level { get; set; }
    public string date { get; set; }
    public string message { get; set; }
  }
}