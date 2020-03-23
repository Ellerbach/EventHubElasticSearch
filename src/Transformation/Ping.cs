using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticTransformation
{
    /// <summary>
    /// Ping class to see if the function is still alive
    /// </summary>
    public static class Ping
  {
    /// <summary>
    /// Simple get function to see if the function is still alive
    /// </summary>
    /// <param name="req">The request</param>
    /// <param name="log">The logger</param>
    /// <param name="context">The context</param>
    /// <returns>OK object if function still alive</returns>
    [FunctionName("Ping")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = null)] HttpRequest req,
        ILogger log,
        ExecutionContext context)
    {
      try
      {
        log.LogInformation("AXA IM EMP Solution Healthcheck.");

        var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var appSettings = new Dictionary<string,string>();
        config.GetSection("Values").Bind(appSettings);

        var healthCheckResponse = new
        {
          status = "OK",
          timestamp = DateTime.Now,
          Appsettings = appSettings,
          ConnectionStrings = new
          {
            SQLConnectionString = config.GetConnectionString("SQLConnectionString")
          }
        };

        return new OkObjectResult(healthCheckResponse);
      }
      catch (Exception ex)
      {
        return new BadRequestObjectResult(ex);
      }
    }
  }
}
