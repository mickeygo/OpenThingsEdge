namespace ThingsEdge.ServerApp.Models;

public sealed class NavCollection
{
    [NotNull]
    public List<NavItem>? Items { get; init; } = new();

    [NotNull]
    public List<NavItem>? Footer { get; init; } = new();
}

/// <summary>
/// 导航项
/// </summary>
public sealed class NavItem
{
    /// <summary>
    /// 导航页面显示文本。
    /// </summary>
    [NotNull]
    public string? Content { get; init; }

    /// <summary>
    /// 导航页面标记名称，导航时会根据该值进行跳转，需保证值唯一。
    /// </summary>
    [NotNull]
    public string? PageTag { get; init; }

    /// <summary>
    /// 导航页面图标。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SymbolRegular Icon { get; init; }

    /// <summary>
    /// 导航页面类型，必须继承了 <see cref="Page"/> 类型。类型命名前缀为 Views.Pages，若命名空间有进一步层级关系，需指定。
    /// </summary>
    [NotNull]
    public string? PageType { get; init; }
}
