namespace ThingsEdge.Contracts;

/// <summary>
/// 数据通道。一个通道对应一种PLC驱动。
/// </summary>
public sealed class ChannelInfo
{
    public int Id { get; set; }

    /// <summary>
    /// 通道名称。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 驱动类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DriverModel Model { get; set; }

    /// <summary>
    /// 设备集合。
    /// </summary>
    [NotNull]
    public List<DeviceInfo> Devices { get; set; } = new();
}
