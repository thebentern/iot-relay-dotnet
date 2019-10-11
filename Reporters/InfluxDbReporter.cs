using System;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
            var report = JsonConvert.DeserializeObject<WeatherStationReport>(json);

            _logger.LogInformation("Recording metric to influx db");
            var client = new InfluxDBClient("http://influxdb:8086", "admin", "admin");
            var metric = new InfluxDatapoint<InfluxValueField>()
            {
                UtcTimestamp = DateTime.UtcNow,
                Precision = TimePrecision.Seconds,
                MeasurementName = "weather-station"
            };

            foreach (var property in report.GetType().GetProperties())
            {
                var value = property.GetValue(report);
                double fieldValue;
                if (value is string)
                    fieldValue = Double.Parse(value.ToString());
                else
                    fieldValue = (double)value;

                metric.Fields.Add(property.Name, new InfluxValueField(fieldValue));
            }

            bool success = await client.PostPointAsync(INFLUX_DB, metric);
        }
    }
}