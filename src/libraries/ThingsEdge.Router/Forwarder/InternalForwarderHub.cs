namespace ThingsEdge.Router.Forwarder;

internal sealed class InternalForwarderHub : IForwarderHub
{
    private readonly HashSet<Type> _forwarderTypes = new();

    public static InternalForwarderHub Instance = new();

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
