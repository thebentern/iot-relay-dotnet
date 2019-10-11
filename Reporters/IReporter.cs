using System.Threading.Tasks;

namespace IotRelay.Service.Reporters
{
    public interface IReporter
    {
        Task ReportAsync(string topic, string json);
    }
}