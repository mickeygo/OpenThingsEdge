namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 注册的 Forwarder 服务集合。
/// </summary>
public sealed class ForwarderRegisterHub : IForwarderServiceHub
{
    private readonly HashSet<string> _serviceKeys = [];

    private static readonly Lazy<IForwarderServiceHub> Instance = new(() => new ForwarderRegisterHub());

    public static IForwarderServiceHub Default => Instance.Value;

    private ForwarderRegisterHub()
    {
    }

    public string[] Keys => [.. _serviceKeys];

    public void Register(string serviceKey)
    {
        if (!_serviceKeys.Add(serviceKey))
        {
            throw new InvalidOperationException($"服务 {nameof(serviceKey)} 已注册，不能再重复注册。");
        }
    }

    public void Remove(string serviceKey)
    {
        _serviceKeys.Remove(serviceKey);
    }
}
