namespace ThingsEdge.Contracts;

/// <summary>
/// 设备信息。一种设备对应指定通道中的一种PLC型号。
/// </summary>
public sealed class DeviceInfo
{
    public int Id { get; set; }

    /// <summary>
    /// 设备名称。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 设备型号。如 S7-200、S7-1200、S7-1500 等。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeviceModel Model { get; set; }

    /// <summary>
    /// 服务器地址。
    /// </summary>
    [NotNull]
    public string? Host { get; set; }

    /// <summary>
    /// 端口。
    /// </summary>
    /// <remarks>不为 0 时表示使用该端口。</remarks>
    public int Port { get; set; }

    /// <summary>
    /// 标记组集合。
    /// </summary>
    [NotNull]
    public List<TagGroup>? TagGroups { get; set; } = new List<TagGroup>();
}
