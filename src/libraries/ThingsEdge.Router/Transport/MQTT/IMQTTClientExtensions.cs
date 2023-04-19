using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace ThingsEdge.Router.Transport.MQTT;

public static class IMQTTClientExtensions
{
    /// <summary>
    /// 订阅主题。
    /// </summary>
    /// <param name="mqttClient"></param>
    /// <param name="topics">订阅的主题集合。</param>
    /// <param name="qualityOfServiceLevel">QoS</param>
    /// <returns></returns>
    public static async Task SubcribeAsync(this IMQTTClient mqttClient, string[] topics, 
        MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce)
    {
        var topicFilters = topics.Select(s => new MqttTopicFilterBuilder().WithTopic(s).WithQualityOfServiceLevel(qualityOfServiceLevel).Build()).ToList();
        await mqttClient.ManagedMqttClient.SubscribeAsync(topicFilters).ConfigureAwait(false);
    }

    /// <summary>
    /// 发布消息。
    /// </summary>
    /// <param name="mqttClient"></param>
    /// <param name="topic">主题。</param>
    /// <param name="payload">要发布的消息内容。</param>
    /// <param name="qualityOfServiceLevel">QoS</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<(bool ok, string err)> PublishAsync(this IMQTTClient mqttClient, string topic, string payload, 
        MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce, CancellationToken cancellationToken = default)
    {
        var result = await mqttClient.ManagedMqttClient.InternalClient.PublishStringAsync(topic, payload, qualityOfServiceLevel, cancellationToken:cancellationToken).ConfigureAwait(false);
        return (result.IsSuccess, result.ReasonString);
    }
}
