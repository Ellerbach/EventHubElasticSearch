using Microsoft.Extensions.Logging;
using Appemulator.Models;

namespace Appemulator.Services
{
  public class LogEmulator : ILogEmulator
  {
    private readonly ILogger<LogEmulator> _logger;

    public LogEmulator(ILogger<LogEmulator> logger)
    {
      _logger = logger;
    }

    public void Run()
    {
      _logger.LogInformation($"Creating a LogEmulator with concrete class injected using constructor injection");
      _logger.LogDebug(20, "Doing hard work! ");
    }
  }
}
