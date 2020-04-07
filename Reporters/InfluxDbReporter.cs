using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AdysTech.InfluxDB.Client.Net;
using IotRelay.Service.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IotRelay.Service.Reporters
{
    [ReporterName("influxdb")]
    public class InfluxDBReporter : IReporter
    {
        public InfluxDBReporter(ILogger logger) => _logger = logger;
        private readonly ILogger _logger;
        public static string INFLUX_DB = DotNetEnv.Env.GetString("INFLUX_DB");

        public async Task ReportAsync(string topic, string json)
        {
            _logger.LogInformation("Recording metric to influx db");
            var client = new InfluxDBClient("http://influxdb:8086", "admin", "admin");
            var metric = new InfluxDatapoint<InfluxValueField>()
            {
                UtcTimestamp = DateTime.UtcNow,
                Precision = TimePrecision.Seconds,
                MeasurementName = topic.Replace("/", String.Empty)
            };

            var jsonPayload = JObject.Parse(json);
            foreach (var token in jsonPayload) 
            {
                try 
                {
                    var fieldValue = Double.TryParse(token.Value.ToString(), out var parsed) ? (double?)parsed : null;
                    _logger.LogInformation($"Metric: {token.Key}. Value: {fieldValue}");
                    metric.Fields.Add(token.Key, new InfluxValueField(fieldValue));
                }
                catch 
                {
                    // Swallow exceptions for individual fields for now
                }
            }
            bool success = await client.PostPointAsync(INFLUX_DB, metric);
        }
    }
}