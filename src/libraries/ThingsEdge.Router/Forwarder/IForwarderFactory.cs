namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 数据传送工厂。
/// </summary>
public interface IForwarderFactory
{
    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <remarks>只返回状态的第一个结果，按 Native > HTTP > MQTT 优先级选取。</remarks>
    /// <param name="message">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
