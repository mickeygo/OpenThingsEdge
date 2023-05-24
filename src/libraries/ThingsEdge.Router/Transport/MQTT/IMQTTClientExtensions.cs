using MQTTnet.Client;
using MQTTnet.Protocol;

namespace ThingsEdge.Router.Transport.MQTT;

public static class IMQTTClientExtensions
{
    /// <summary>
    /// 发布消息。
    /// </summary>
    /// <param name="mqttClient"></param>
    /// <param name="topic">主题。</param>
    /// <param name="payload">要发布的消息内容。</param>
    /// <param name="qos">QoS</param>
    /// <param name="retain">是否保留数据，默认 false</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<(bool ok, string err)> PublishAsync(this IMQTTClient mqttClient, string topic, string payload,
       MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default)
    {
        var result = await mqttClient.MqttClient.PublishStringAsync(topic, payload, qos, retain, cancellationToken).ConfigureAwait(false);
        return (result.IsSuccess, result.ReasonString);
    }
}
