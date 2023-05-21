namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 后台服务
/// </summary>
internal sealed class MQTTHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMQTTMessageReceivedHandler _msgRecevHandler;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    private IMQTTClient? _mqttClient;

    public MQTTHostedService(IServiceScopeFactory serviceScopeFactory,
        IMQTTMessageReceivedHandler msgRecevHandler, 
        IOptions<MQTTClientOptions> mqttClientOptions, 
        ILogger<MQTTHostedService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _msgRecevHandler = msgRecevHandler;
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Host 服务开启");

        _mqttClient = MQTTClientFactory.Create(_mqttClientOptions);
        _mqttClient.ManagedMqttClient.ApplicationMessageReceivedAsync += async (args) =>
        {
            await _msgRecevHandler.HandleAsync(args, cancellationToken).ConfigureAwait(false);
        };

        await _mqttClient.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            _logger.LogInformation("MQTT Host 服务停止");

            await _mqttClient.StopAsync().ConfigureAwait(false);
            _mqttClient.Dispose();
        }
    }
}
