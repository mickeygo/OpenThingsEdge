using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Router.Transport.MQTT;

public static class IMQTTManagedClientExtensions
{
    /// <summary>
    /// 订阅主题。
    /// </summary>
    /// <remarks>
    /// 关于 Topic 规范：首字符不要使用斜杠；
    /// 不要使用空格；
    /// 保持简短；
    /// 只使用ASCII字符；
    /// 将唯一标识符或ClientID嵌入到主题中，方便识别消息的发送方，便于调试和鉴权；
    /// 不要订阅#（多级通配符）；
    /// 使用定义精确的 Topic 而不要使用定义模糊的Topic。
    /// </remarks>
    /// <param name="mqttClient"></param>
    /// <param name="topics">订阅的主题集合。</param>
    /// <param name="qos">QoS</param>
    /// <returns></returns>
    public static async Task SubcribeAsync(this IMQTTManagedClient mqttClient, string[] topics, 
        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
    {
        var topicFilters = topics.Select(s => new MqttTopicFilterBuilder().WithTopic(s).WithQualityOfServiceLevel(qos).Build()).ToList();
        await mqttClient.ManagedMqttClient.SubscribeAsync(topicFilters).ConfigureAwait(false);
    }

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
    public static async Task<(bool ok, string err)> PublishAsync(this IMQTTManagedClient mqttClient, string topic, string payload, 
        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce, bool retain = false, CancellationToken cancellationToken = default)
    {
        var result = await mqttClient.ManagedMqttClient.InternalClient.PublishStringAsync(topic, payload, qos, retain, cancellationToken).ConfigureAwait(false);
        return (result.IsSuccess, result.ReasonString);
    }

    /// <summary>
    /// 发布消息，消息存储在内部的队列中，一旦客户端连接成功会立即发送。
    /// </summary>
    /// <remarks>
    /// 关于 Topic 规范：首字符不要使用斜杠；
    /// 不要使用空格；
    /// 保持简短；
    /// 只使用ASCII字符；
    /// 将唯一标识符或ClientID嵌入到主题中，方便识别消息的发送方，便于调试和鉴权；
    /// 不要订阅#（多级通配符）；
    /// 使用定义精确的 Topic 而不要使用定义模糊的Topic。
    /// </remarks>
    /// <param name="mqttClient"></param>
    /// <param name="topic">主题。</param>
    /// <param name="payload">要发布的消息内容。</param>
    /// <param name="qos">QoS</param>
    /// <param name="retain">是否保留数据，默认 false</param>
    /// <returns></returns>
    public static async Task EnqueueAsync(this IMQTTManagedClient mqttClient, string topic, string payload,
         MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce, bool retain = false)
    {
        await mqttClient.ManagedMqttClient.EnqueueAsync(topic, payload, qos, retain).ConfigureAwait(false);
    }
}
