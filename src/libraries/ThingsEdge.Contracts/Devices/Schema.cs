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
    /// <remarks>若标记隶属于设备，那么标记组可为空。</remarks>
    public string? TagGroupName { get; set; }
}
