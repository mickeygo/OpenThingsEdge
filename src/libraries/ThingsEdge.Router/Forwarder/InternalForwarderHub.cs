namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 内部注册的 Forwarder 对象服务集合。
/// </summary>
/// <remarks>采用 <see cref="ActivatorUtilities.GetServiceOrCreateInstance"/> 获取服务对象。</remarks>
internal sealed class InternalForwarderHub : IForwarderHub
{
    private readonly HashSet<Type> _forwarderTypes = [];

    private static readonly Lazy<IForwarderHub> Instance = new(() => new InternalForwarderHub());

    public static IForwarderHub Default = Instance.Value;

    private InternalForwarderHub()
    {
    }

    public void Register<T>() where T : IForwarder
    {
        _forwarderTypes.Add(typeof(T));
    }

    public void Remove<T>() where T : IForwarder
    {
        _forwarderTypes.Remove(typeof(T));
    }

    public IReadOnlyList<IForwarder> ResloveAll(IServiceProvider serviceProvider)
    {
        List<IForwarder> forwarders = new(_forwarderTypes.Count);
        foreach (var forwarderType in _forwarderTypes)
        {
            var obj = (IForwarder)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, forwarderType);
            forwarders.Add(obj);
        }

        return forwarders;
    }
}
