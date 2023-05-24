using Nito.AsyncEx;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Router.Forwarder;

internal sealed class MqttClientForwarder : IForwarder, IAsyncDisposable
{
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;
    private readonly AsyncLock _asyncLock = new();
    private IMQTTManagedClient? _managedMqttClient;

    public MqttClientForwarder(IOptions<MQTTClientOptions> mqttClientOptions, ILogger<MqttClientForwarder> logger)
    {
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            // 双检锁 
            if (_managedMqttClient is null)
            {
                using (await _asyncLock.LockAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (_managedMqttClient is null)
                    {
                        _managedMqttClient = MQTTClientFactory.CreateManagedClient(_mqttClientOptions);
                        await _managedMqttClient.StartAsync();
                    }
                }
            }

            // Topic => {ChannelName}/{DeviceName}/[{TagGroupName}], eg: line01/device01/op010
            StringBuilder topic = new();
            topic.AppendFormat("{0}/{1}", message.Schema.ChannelName, message.Schema.DeviceName);
            if (!string.IsNullOrEmpty(message.Schema.TagGroupName))
            {
                topic.AppendFormat("/{0}", message.Schema.TagGroupName);
            }

            await _managedMqttClient.EnqueueAsync(topic.ToString().ToLower(), JsonSerializer.Serialize(message)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttClientForwarder] MQTT 服务发布异常。");
        }
     
        // MQTT 忽略返回结果。
        return ResponseResult.FromOk(new ResponseMessage
        {
            Request = message,
        }, ResponseResult.ResponseSource.MQTT);
    }

    public async ValueTask DisposeAsync()
    {
        if (_managedMqttClient is not null)
        {
            await _managedMqttClient.StopAsync();
            _managedMqttClient.Dispose();
        }
    }
}
