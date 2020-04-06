using System;
using System.Net.Http;
using System.Threading.Tasks;
using IotRelay.Service.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace IotRelay.Service.Reporters
{
    [ReporterName("wunderground")]
    public class WUndergroundReporter : IReporter
    {
        public WUndergroundReporter(ILogger logger) => _logger = logger;

        private readonly ILogger _logger;
        
        private static IAsyncPolicy TimeoutPolicy = Policy
            .TimeoutAsync(5);

        private static AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8) });

        private async Task UploadReportWeatherUnderground(string url)
        {
            _logger.LogInformation("Reporting conditions to Weather Underground...");
            _logger.LogInformation($"GET: {url}");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", ".NET Climate Service Reporter");
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetStringAsync(url).ConfigureAwait(false);
                _logger.LogInformation(response);
            }
        }
        
        public async Task ReportAsync(string topic, string jsonData)
        {
            var report = JsonConvert.DeserializeObject<WeatherStationReport>(jsonData);

            await TimeoutPolicy.ExecuteAsync(async () => 
                await UploadReportWeatherUnderground(report.BuildWeatherStationUrl()));
        }
    }
}