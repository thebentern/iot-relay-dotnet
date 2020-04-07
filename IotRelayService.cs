using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Nito.AsyncEx;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IotRelay.Service
{
    public class IotRelayService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ReporterController _reportController;
        private IEnumerable<IotChannel> _iotChannels;

        public IotRelayService(ILogger<IotRelayService> logger) 
        {
            _logger = logger;
            _reportController = new ReporterController(_logger);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(IotRelayService)} is starting.");
            
            _iotChannels = JsonConvert.DeserializeObject<IEnumerable<IotChannel>>(await File.ReadAllTextAsync("./iot-channels.jsonc"));

            DotNetEnv.Env.Load();
            IMqttClient mqttClient = CreateMqttClient();
            IMqttClientOptions options = CreateMqttClientOptions();

            SetupMqttConnection(mqttClient);
            SetupMqttDisconnection(mqttClient, options);
            mqttClient.ApplicationMessageReceived += OnMessageReceived;
            await mqttClient.ConnectAsync(options);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(IotRelayService)} is stopping.");
            return Task.CompletedTask;
        }

        private void OnMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            _logger.LogInformation($"Received message on topic: {e.ApplicationMessage.Topic}{Environment.NewLine}");
            _logger.LogInformation($"Payload = {payload}");

            foreach (var targetName in _iotChannels.Where(c => c.Topic.Equals(e.ApplicationMessage.Topic, StringComparison.OrdinalIgnoreCase))
                .SelectMany(c => c.Targets))
            {
                _logger.LogInformation($"Sending to target: {targetName}");
                AsyncContext.Run(async () => await _reportController.ReportMessageAsync(targetName, e.ApplicationMessage.Topic, payload));
            }
            _logger.LogInformation(Environment.NewLine);
        }

        private void SetupMqttDisconnection(IMqttClient mqttClient, IMqttClientOptions options)
        {
            mqttClient.Disconnected += async (s, e) =>
            {
                _logger.LogInformation("Disconnected to server");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options);
                }
                catch
                {
                    _logger.LogInformation("Reconnecting failed");
                }
            };
        }

        private void SetupMqttConnection(IMqttClient mqttClient)
        {
            mqttClient.Connected += async (s, e) =>
            {
                _logger.LogInformation("Connected to server");
                
                var topicFilters = _iotChannels.ToList().Select(c => new TopicFilterBuilder().WithTopic(c.Topic).Build());
                await mqttClient.SubscribeAsync(topicFilters);

                var friendlyTopics = String.Join(", ", _iotChannels.Select(c => c.Topic).ToArray());
                _logger.LogInformation($"Subscribed to topics: {friendlyTopics}");
            };
        }

        private static IMqttClientOptions CreateMqttClientOptions()
        {
            return new MqttClientOptionsBuilder()
                .WithTcpServer(DotNetEnv.Env.GetString("MQTT_HOST"), 1883)
                .Build();
        }

        private static IMqttClient CreateMqttClient()
        {
            var factory = new MqttFactory();
            return factory.CreateMqttClient();
        }

        public void Dispose() { }
    }
}
