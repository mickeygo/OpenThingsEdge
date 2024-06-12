using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Contrib.Mqtt.Transport;

/// <summary>
/// MQTT 客户端。
/// </summary>
public interface IMQTTManagedClient : IDisposable
{
    /// <summary>
    /// MQTT 客户端
    /// </summary>
    public IManagedMqttClient ManagedMqttClient { get; }

    /// <summary>
    /// 启动服务。
    /// </summary>
    /// <remarks>注：启动后尝试连接Broker，连接成功后会订阅主题，因此在开启前选先订阅好主题。</remarks>
    /// <returns></returns>
    Task StartAsync();

    Task StopAsync(bool cleanDisconnect = true);
}
