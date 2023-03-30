namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 后台服务
/// </summary>
internal sealed class MQTTHostedService : IHostedService
{
    private readonly IMQTTMessageReceivedHandler _msgRecevHandler;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    private IMQTTClient? _mqttClient;

    public MQTTHostedService(IMQTTMessageReceivedHandler msgRecevHandler, 
        IOptions<MQTTClientOptions> mqttClientOptions, 
        ILogger<MQTTHostedService> logger)
    {
        _msgRecevHandler = msgRecevHandler;
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _mqttClient = MQTTClientFactory.Create(_mqttClientOptions);
        _mqttClient.ManagedMqttClient.ApplicationMessageReceivedAsync += async (args) =>
        {
            await _msgRecevHandler.HandleAsync(args, cancellationToken);
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
