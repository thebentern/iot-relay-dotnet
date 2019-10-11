using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IotRelay.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.AddConsole();
                        loggingBuilder.AddFilter(f => f >= LogLevel.Information);
                    });
                services.AddSingleton<IHostedService, IotRelayService>();
            });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
