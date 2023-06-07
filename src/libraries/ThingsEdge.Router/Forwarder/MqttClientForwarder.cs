using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Router.Forwarder;

internal sealed partial class MqttClientForwarder : IForwarder
{
    private readonly IMQTTManagedClient _managedMqttClient;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    public ForworderSource Source => ForworderSource.MQTT;

    public MqttClientForwarder(IMQTTManagedClient managedMqttClient,
        IOptionsMonitor<MQTTClientOptions> mqttClientOptions, 
        ILogger<MqttClientForwarder> logger)
    {
        _managedMqttClient = managedMqttClient;
        _mqttClientOptions = mqttClientOptions.CurrentValue;
        _logger = logger;
    }

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
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

    [GeneratedRegex("{ChannelName}|{DeviceName}|{TagGroupName}", RegexOptions.IgnoreCase)]
    private static partial Regex TopicRegex();
}
