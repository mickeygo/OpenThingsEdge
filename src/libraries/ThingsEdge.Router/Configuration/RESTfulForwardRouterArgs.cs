namespace ThingsEdge.Router.Configuration;

/// <summary>
/// RESTful 路由参数
/// </summary>
public sealed class RESTfulForwardRouterArgs
{
    /// <summary>
    /// 加载的数据头。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; init; }

    public TagFlag Flag { get; init; }

    [NotNull]
    public string? TagName { get; init; }
}
