namespace ThingsEdge.Router.Forwarders;

internal sealed class RequestForwarderProvider(IServiceProvider serviceProvider) : IRequestForwarderProvider, ISingletonDependency
{
    public IRequestForwarder? GetForwarder()
    {
        return serviceProvider.GetService<IRequestForwarder>();
    }
}
