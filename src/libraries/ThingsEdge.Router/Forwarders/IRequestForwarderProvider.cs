namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 数据发送请求执行者。
/// </summary>
/// <remarks>Request 请求只允许最多有一个处理服务。</remarks>
public interface IRequestForwarderProvider
{
    /// <summary>
    /// 获取发送请求对象，若没有注册，则为 null。
    /// </summary>
    /// <returns></returns>
    IRequestForwarder? GetForwarder();
}
