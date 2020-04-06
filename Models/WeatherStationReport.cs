using System;
using System.Text;

namespace IotRelay.Service.Models
{
    public class WeatherStationReport
    {
        public static string PWS_ID = DotNetEnv.Env.GetString("PWS_ID");
        public static string PWS_KEY = DotNetEnv.Env.GetString("PWS_KEY");
        private const double PSI_TO_INHG_COEFFICIENT = 2.03602;

        public string voltage { get; set; }
        public string temperature { get; set; }
        public string humidity { get; set; }
        public string barometricPressure { get; set; }

        public double barometricPressureInHg => Double.Parse(barometricPressure) * PSI_TO_INHG_COEFFICIENT;
        public double dewpoint => CaculateDewpoint(Double.Parse(temperature), Double.Parse(humidity));
        
        private static double CaculateDewpoint(double temperature, double humidity)
        {
            double temp = (temperature - 32) / 1.8;
            
            double result =  (temp - (14.55 + 0.114 * temp) * (1 - (0.01 * humidity)) - Math.Pow(((2.5 + 0.007 * temp) * (1 - (0.01 * humidity))),3) - (15.9 + 0.117 * temp) * Math.Pow((1 - (0.01 * humidity)), 14));

            double value = result * (9.0/5.0);
            
            return value + 32.0;
        }

        public string BuildWeatherStationUrl()
        {
            var builder = new StringBuilder("https://weatherstation.wunderground.com/weatherstation/updateweatherstation.php?");
            builder.Append($"ID={PWS_ID}");
            builder.Append($"&PASSWORD={PWS_KEY}");
            builder.Append("&action=updateraw");
            builder.Append("&dateutc=now");

            if (this.temperature != null)
                builder.Append($"&tempf={this.temperature}");
                
            if (this.humidity != null)
                builder.Append($"&humidity={this.humidity}");

            if (this.barometricPressure != null)
                builder.Append($"&baromin={this.barometricPressureInHg}");

            if (this.temperature != null && this.humidity != null)
                builder.Append($"&dewptf={this.dewpoint}");

            return builder.ToString();
        }
    }
}