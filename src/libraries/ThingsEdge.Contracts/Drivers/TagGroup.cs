namespace ThingsEdge.Contracts;

/// <summary>
/// 标记组。
/// </summary>
public sealed class TagGroup
{
    /// <summary>
    /// 唯一值。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标记组名。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; } = string.Empty;

    /// <summary>
    /// 隶属于分组的标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? Tags { get; set; } = new();
}
