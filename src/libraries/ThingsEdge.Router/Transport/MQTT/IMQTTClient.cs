using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// MQTT 客户端。
/// </summary>
public interface IMQTTClient : IDisposable
{
    /// <summary>
    /// MQTT 客户端
    /// </summary>
    IMqttClient MqttClient { get; }

    /// <summary>
    /// 连接服务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
