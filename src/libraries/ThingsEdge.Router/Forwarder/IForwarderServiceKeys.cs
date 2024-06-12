namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 注册的 Forwarder 对象服务关键字。
/// </summary>
/// <remarks>采用 <see cref="ServiceDescriptor.ServiceKey"/> 获取服务对象。</remarks>
public interface IForwarderServiceKeys
{
    /// <summary>
    /// 获取注册的所有服务关键字。
    /// </summary>
    string[] Keys { get; }

    /// <summary>
    /// 注册服务，若服务已存在，则不会再添加。
    /// </summary>
    /// <param name="serviceKey">服务关键字</param>
    void Register(string serviceKey);

    /// <summary>
    /// 移除服务.
    /// </summary>
    /// <param name="serviceKey">服务关键字</param>
    void Remove(string serviceKey);
}
