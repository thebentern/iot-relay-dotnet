using System.Collections.Generic;

namespace IotRelay.Service
{
    public class IotChannel
    {
        public string Topic { get; set; }
        public IEnumerable<string> Targets { get; set; }
    }
}