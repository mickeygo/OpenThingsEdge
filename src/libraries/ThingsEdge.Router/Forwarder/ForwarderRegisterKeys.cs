namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 注册的 Forwarder 对象服务关键字。
/// </summary>
public sealed class ForwarderRegisterKeys : IForwarderServiceKeys
{
    private readonly HashSet<string> _serviceKeys = [];

    private static readonly Lazy<IForwarderServiceKeys> Instance = new(() => new ForwarderRegisterKeys());

    public static readonly IForwarderServiceKeys Default = Instance.Value;

    private ForwarderRegisterKeys()
    {
    }

    public string[] Keys => [.. _serviceKeys];

    public void Register(string serviceKey)
    {
        _serviceKeys.Add(serviceKey);
    }

    public void Remove(string serviceKey)
    {
        _serviceKeys.Remove(serviceKey);
    }
}
