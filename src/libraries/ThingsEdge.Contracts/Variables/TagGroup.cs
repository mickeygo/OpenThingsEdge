namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记组。
/// </summary>
public sealed class TagGroup
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    public string TagGroupId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 标记组名。
    /// </summary>
    [NotNull]
    public string? Name { get; init; }

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; init; } = string.Empty;

    /// <summary>
    /// 隶属于分组的标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? Tags { get; init; } = [];

    /// <summary>
    /// 隶属于分组的回写标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? CallbackTags { get; init; } = [];
}
