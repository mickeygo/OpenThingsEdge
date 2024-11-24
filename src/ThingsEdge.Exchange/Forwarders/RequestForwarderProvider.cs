namespace ThingsEdge.Exchange.Forwarders;

internal sealed class RequestForwarderProvider(IServiceProvider serviceProvider) : IRequestForwarderProvider
{
    public IRequestForwarder? GetForwarder()
    {
        return serviceProvider.GetService<IRequestForwarder>();
    }
}
