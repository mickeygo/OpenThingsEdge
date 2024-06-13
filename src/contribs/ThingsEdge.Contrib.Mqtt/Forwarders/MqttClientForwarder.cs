using ThingsEdge.Contrib.Mqtt.Transport;
using ThingsEdge.Router.Forwarders;
using ThingsEdge.Router.Transport.MQTT;

namespace ThingsEdge.Contrib.Mqtt.Forwarders;

internal sealed class MqttClientForwarder : IRequestForwarder
{
    private readonly IMQTTManagedClient _managedMqttClient;
    private readonly MQTTClientOptions _mqttClientOptions;
    private readonly ILogger _logger;

    public MqttClientForwarder(IMQTTManagedClient managedMqttClient,
        IOptions<MQTTClientOptions> mqttClientOptions,
        ILogger<MqttClientForwarder> logger)
    {
        _managedMqttClient = managedMqttClient;
        _mqttClientOptions = mqttClientOptions.Value;
        _logger = logger;
    }

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var topic = _mqttClientOptions.TopicFormaterFunc?.Invoke(new()
            {
                Schema = message.Schema,
                Flag = message.Flag,
                TagName = message.Self().TagName,
            }) ?? MQTTClientTopicFormater.Default(message.Schema, _mqttClientOptions.TopicFormater, _mqttClientOptions.TopicFormatMatchLower);

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
            State = _mqttClientOptions.SuccessCode,
            Request = message,
        });
    }
}
