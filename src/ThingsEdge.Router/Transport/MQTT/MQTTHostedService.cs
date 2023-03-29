namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 后台服务
/// </summary>
internal sealed class MQTTHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    private IMQTTClient? _mqttClient;

    public MQTTHostedService(IServiceProvider serviceProvider, IOptions<MQTTClientOptions> mqttClientOptions, ILogger<MQTTHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _mqttClient = MQTTClientFactory.Create(_mqttClientOptions);
        _mqttClient.ManagedMqttClient.ApplicationMessageReceivedAsync += async (args) =>
        {
            var handler = _serviceProvider.GetRequiredService<IMQTTMessageReceivedHandler>();
            await handler.HandleAsync(args, cancellationToken);
        };
        await _mqttClient.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            await _mqttClient.StopAsync();
            _mqttClient.Dispose();
        }
    }
}
