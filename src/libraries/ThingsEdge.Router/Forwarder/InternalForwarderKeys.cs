namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 内部注册的 Forwarder 对象服务关键字。
/// </summary>
/// <remarks>采用 <see cref="ServiceDescriptor.ServiceKey"/> 获取服务对象。</remarks>
internal sealed class InternalForwarderKeys
{
    private readonly HashSet<string> _serviceKeys = [];

    private static readonly Lazy<InternalForwarderKeys> Instance = new(() => new InternalForwarderKeys());

    public static InternalForwarderKeys Default = Instance.Value;

    private InternalForwarderKeys()
    {
    }

    /// <summary>
    /// 获取注册的所有服务关键字。
    /// </summary>
    public string[] Keys => [.. _serviceKeys];

    /// <summary>
    /// 注册服务，若服务已存在，则不会再添加。
    /// </summary>
    /// <param name="serviceKey">服务关键字</param>
    public void Register(string serviceKey)
    {
        _serviceKeys.Add(serviceKey);
    }

    /// <summary>
    /// 移除服务.
    /// </summary>
    /// <param name="serviceKey">服务关键字</param>
    public void Remove(string serviceKey)
    {
        _serviceKeys.Remove(serviceKey);
    }
}
