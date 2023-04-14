using ThingsEdge.Contracts.Devices;

namespace ThingsEdge.Contracts;

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
    /// 设备名称。
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
    /// <remarks>不为 0 时表示使用该端口。</remarks>
    public int Port { get; init; }

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; init; } = string.Empty;

    /// <summary>
    /// 标记组集合。
    /// </summary>
    [NotNull]
    public List<TagGroup>? TagGroups { get; init; } = new();

    /// <summary>
    /// 隶属于设备的标记集合。
    /// </summary>
    [NotNull]
    public List<Tag>? Tags { get; init; } = new();

    /// <summary>
    /// 从所有标记分组中获取指定标识的标记集合。
    /// </summary>
    /// <param name="flag">标识</param>
    /// <returns></returns>
    public List<Tag> GetTagsFromGroups(TagFlag flag)
    {
        return TagGroups.SelectMany(s => s.Tags.Where(t => t.Flag == flag)).ToList();
    }
}
