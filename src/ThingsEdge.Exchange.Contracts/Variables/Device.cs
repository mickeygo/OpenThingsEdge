namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 设备信息。一种设备对应指定通道中的一种PLC型号。
/// </summary>
public sealed class Device
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    public string DeviceId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 设备唯一名称。
    /// </summary>
    [NotNull]
    public string? Name { get; init; }

    /// <summary>
    /// 设备驱动型号。如 S7-200、S7-1200、S7-1500 等。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DriverModel Model { get; init; }

    /// <summary>
    /// 服务器地址。
    /// </summary>
    [NotNull]
    public string? Host { get; init; }

    /// <summary>
    /// 端口。
    /// </summary>
    /// <remarks>为 0 时表示使用驱动默认的端口。</remarks>
    public int Port { get; init; }

    /// <summary>
    /// 允许一次性读取的最大 PDU 长度（byte数量），为 0 时会使用全局配置。
    /// </summary>
    /// <remarks>
    /// 西门子 S7 协议批量读取时 PDU 超过最大长度会出现异常，这时可指定最大长度，一次批量读取时内部按最大长度切分到多次读取。
    /// </remarks>
    public int MaxPDUSize { get; init; }

    /// <summary>
    /// 设备连接使用的线程池，优先级高于全局配置，为 0 时会使用全局配置。
    /// </summary>
    public int PoolSize { get; init; }

    /// <summary>
    /// 设备要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; init; } = string.Empty;

    /// <summary>
    /// 标记组集合。
    /// </summary>
    [NotNull]
    public List<TagGroup>? TagGroups { get; init; } = [];

    /// <summary>
    /// 隶属于设备的标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? Tags { get; init; } = [];

    /// <summary>
    /// 隶属于设备的回写标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? CallbackTags { get; init; } = [];

    /// <summary>
    /// 从所有顶层标记分组中获取指定标识的标记集合。
    /// </summary>
    /// <param name="flag">标识</param>
    /// <returns></returns>
    public List<Tag> GetTagsFromGroups(TagFlag flag)
    {
        return [.. TagGroups.SelectMany(s => s.Tags.Where(t => t.Flag == flag))];
    }

    /// <summary>
    /// 从设备的所有顶层标记中（包括设备标记和分组标记）中获取指定标识的标记集合。
    /// </summary>
    /// <param name="flag">标识</param>
    /// <returns></returns>
    public List<Tag> GetAllTags(TagFlag flag)
    {
        return
        [
            .. TagGroups.SelectMany(s => s.Tags.Where(t => t.Flag == flag)),
            .. Tags.Where(s => s.Flag == flag),
        ];
    }

    /// <summary>
    /// 获取标记属于的分组，若未找到标记或是标记不属于分组，返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    public TagGroup? GetTagGroup(string tagId)
    {
        if (Tags.Any(s => s.TagId == tagId))
        {
            return null;
        }

        var tagGroup = TagGroups.FirstOrDefault(s => s.Tags.Any(t => t.TagId == tagId));
        return tagGroup;
    }
}
