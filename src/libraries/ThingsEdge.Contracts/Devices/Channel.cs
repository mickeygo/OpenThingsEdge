namespace ThingsEdge.Contracts;

/// <summary>
/// 数据通道。一个通道对应一种PLC驱动。
/// </summary>
public sealed class Channel
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    public string ChannelId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 通道名称。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 驱动类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChannelModel Model { get; set; } = ChannelModel.Siemens;

    /// <summary>
    /// 标记要旨，可用于设置重要信息。
    /// </summary>
    [NotNull]
    public string? Keynote { get; set; } = string.Empty;

    /// <summary>
    /// 设备集合。
    /// </summary>
    [NotNull]
    public List<Device> Devices { get; set; } = new();
}
