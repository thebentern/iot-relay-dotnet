using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IotRelay.Service.Reporters;
using Microsoft.Extensions.Logging;

namespace IotRelay.Service
{
    internal class ReporterController
    {
        public ReporterController(ILogger logger)
        {
            _logger = logger;
            _reporterMap = GetReporters(logger);
        }
        private readonly ILogger _logger;

        private Dictionary<string, IReporter> _reporterMap;

        private Dictionary<string, IReporter> GetReporters(ILogger logger) 
        {
            var reporters = new Dictionary<string, IReporter>();
            var reporterTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IReporter).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (var type in reporterTypes) 
            {
                reporters.Add(type.GetCustomAttributes(typeof(ReporterNameAttribute), false).Cast<ReporterNameAttribute>().Single().Name,
                    (IReporter)Activator.CreateInstance(type, _logger));
            }
            return reporters;
        }

    public async Task ReportMessageAsync(string name, string topic, string jsonMessage) => 
        await this._reporterMap[name].ReportAsync(topic, jsonMessage);
  }
}