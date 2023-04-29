namespace ThingsEdge.Router.Management;

internal sealed class RouterBuilder : IRouterBuilder
{
    public RouterBuilder(IHostBuilder builder)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public IHostBuilder Builder { get; }

    public ICollection<Assembly> EventAssemblies { get; } = new List<Assembly>();
}
