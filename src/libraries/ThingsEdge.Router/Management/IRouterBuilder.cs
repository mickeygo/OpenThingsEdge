namespace ThingsEdge.Router;

/// <summary>
/// Router Builder 接口。
/// </summary>
public interface IRouterBuilder
{
    /// <summary>
    /// Gets the builder.
    /// </summary>
    IHostBuilder Builder { get; }

    /// <summary>
    /// 要自动注入到 EventBus 的程序集。
    /// </summary>
    ICollection<Assembly> EventAssemblies { get; }
}
