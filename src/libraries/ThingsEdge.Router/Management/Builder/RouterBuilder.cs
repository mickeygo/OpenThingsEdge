namespace ThingsEdge.Router.Management;

internal sealed class RouterBuilder(IHostBuilder builder) : IRouterBuilder
{
    public IHostBuilder Builder { get; } = builder ?? throw new ArgumentNullException(nameof(builder));

    public ICollection<Assembly> EventAssemblies { get; } = [];
}
