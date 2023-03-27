namespace ThingsEdge.Contracts;

public sealed class Schema
{
    /// <summary>
    /// 通道名称。
    /// </summary>
    [NotNull]
    public string? ChannelName { get; set; }

    /// <summary>
    /// 设备名称。
    /// </summary>
    [NotNull]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 标记组名称。
    /// </summary>
    [NotNull]
    public string? TagGroupName { get; set; }
}
