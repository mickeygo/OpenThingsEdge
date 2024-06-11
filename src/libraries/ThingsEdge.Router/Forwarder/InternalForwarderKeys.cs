namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 内部注册的 Forwarder 对象服务关键字。
/// </summary>
internal sealed class InternalForwarderKeys
{
    private readonly HashSet<string> _serviceKeys = [];

    private static readonly Lazy<InternalForwarderKeys> Instance = new(() => new InternalForwarderKeys());

    public static InternalForwarderKeys Default = Instance.Value;

    /// <summary>
    /// 获取注册的所有服务关键字。
    /// </summary>
    public string[] Keys => [.. _serviceKeys];

    /// <summary>
    /// 注册服务。
    /// </summary>
    /// <param name="serviceKey"></param>
    public void Register(string serviceKey)
    {
        _serviceKeys.Add(serviceKey);
    }

    /// <summary>
    /// 移除服务
    /// </summary>
    /// <param name="serviceKey"></param>
    public void Remove(string serviceKey)
    {
        _serviceKeys.Remove(serviceKey);
    }
}
