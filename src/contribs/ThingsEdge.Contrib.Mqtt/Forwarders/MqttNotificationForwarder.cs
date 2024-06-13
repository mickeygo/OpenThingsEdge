using ThingsEdge.Contrib.Mqtt.Transport;
using ThingsEdge.Router.Forwarders;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Contrib.Mqtt.Forwarders;

/// <summary>
/// 基于 MQTT 协议的转发通知服务。
/// </summary>
internal sealed class MqttNotificationForwarder : INotificationForwarder
{
    private readonly IMQTTManagedClient _managedMqttClient;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    public MqttNotificationForwarder(IMQTTManagedClient managedMqttClient,
       IOptions<MQTTClientOptions> mqttClientOptions,
       ILogger<MqttNotificationForwarder> logger)
    {
        _managedMqttClient = managedMqttClient;
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task PublishAsync(RequestMessage message, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default)
    {
        try
        {
            // 记录请求的数据。
            _logger.LogInformation("RequestId: {RequestId}, 设备: {DeviceName}, 分组: {TagGroupName}, 请求值: {Values}",
                message.RequestId, message.Schema.DeviceName, message.Schema.TagGroupName,
                JsonSerializer.Serialize(message.Values.Select(s => new { s.TagName, s.Address, s.DataType, s.Length, s.Value })));

            var topic = _mqttClientOptions.TopicFormaterFunc?.Invoke(new()
            {
                Schema = message.Schema,
                Flag = message.Flag,
                TagName = message.Self().TagName,
            }) ?? MQTTClientTopicFormater.Default(message.Schema, _mqttClientOptions.TopicFormater, _mqttClientOptions.TopicFormatMatchLower);
            var payload = _mqttClientOptions.PayloadFormaterFunc?.Invoke(message) ?? JsonSerializer.Serialize(message);
            var (ok, err) = await _managedMqttClient.PublishAsync(topic, payload, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (ok)
            {
                _logger.LogInformation("RequestId: {RequestId}, MQTT 推送数据完成", message.RequestId);
            }
            else
            {
                _logger.LogWarning("RequestId: {RequestId}, MQTT 推送数据失败，错误：{Err}", message.RequestId, err);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[MqttNotificationForwarder] MQTT 推送超时");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttNotificationForwarder] MQTT 推送异常");
        }
    }
}
