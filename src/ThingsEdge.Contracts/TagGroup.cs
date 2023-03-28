namespace ThingsEdge.Contracts;

/// <summary>
/// 标记组。
/// </summary>
public sealed class TagGroup
{
    public int Id { get; set; }

    /// <summary>
    /// 标记组名。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? Tags { get; set; } = new();
}
