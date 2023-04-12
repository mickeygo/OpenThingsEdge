namespace ThingsEdge.Router.Forwarder;

public interface IMqttForwarder
{
    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(RequestMessage message, CancellationToken cancellationToken);
}
