using Nito.AsyncEx;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Router.Forwarder;

internal sealed partial class MqttClientForwarder : IForwarder, IAsyncDisposable
{
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;
    private readonly AsyncLock _asyncLock = new();
    private IMQTTManagedClient? _managedMqttClient;

    public ForworderSource Source => ForworderSource.MQTT;

    public MqttClientForwarder(IOptionsMonitor<MQTTClientOptions> mqttClientOptions, ILogger<MqttClientForwarder> logger)
    {
        _mqttClientOptions = mqttClientOptions.CurrentValue;
        _logger = logger;
    }

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            // 双检锁（本实例需注册为单例）
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

            // 默认格式 {ChannelName}/{DeviceName}/{TagGroupName}, eg: line01/device01/op010
            var topicFormater = _mqttClientOptions.TopicFormater ?? "{ChannelName}/{DeviceName}/{TagGroupName}";
            var match = TopicRegex().Replace(topicFormater, match =>
            {
                return match.Value switch
                {
                    "{ChannelName}" => MatchToLower(message.Schema.ChannelName),
                    "{DeviceName}" => MatchToLower(message.Schema.DeviceName),
                    "{TagGroupName}" => MatchToLower(message.Schema.TagGroupName ?? ""),
                    _ => "",
                };
            });

            // 移除首尾斜杠
            var topic = match.Trim('/');
            await _managedMqttClient.EnqueueAsync(topic, JsonSerializer.Serialize(message)).ConfigureAwait(false);
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
        }, Source);


        string MatchToLower(string str)
        {
            if (_mqttClientOptions.TopicFormatMatchLower)
            {
                return str.ToLower();
            }

            return str;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_managedMqttClient is not null)
        {
            await _managedMqttClient.StopAsync();
            _managedMqttClient.Dispose();
        }
    }

    [GeneratedRegex("{ChannelName}|{DeviceName}|{TagGroupName}", RegexOptions.IgnoreCase)]
    private static partial Regex TopicRegex();
}
