using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 接收到消息后事件处理者。
/// </summary>
public interface IMQTTMessageReceivedHandler
{
    /// <summary>
    /// 处理事件。
    /// </summary>
    /// <param name="message">要处理的消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(MqttApplicationMessageReceivedEventArgs message, CancellationToken cancellationToken);
}
