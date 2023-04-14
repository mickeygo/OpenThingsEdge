namespace ThingsEdge.Contracts;

/// <summary>
/// 用于构建数据头。
/// </summary>
public sealed class Schema
{
    /// <summary>
    /// 通道名称。
    /// </summary>
    [NotNull]
    public string? ChannelName { get; init; }

    /// <summary>
    /// 设备名称。
    /// </summary>
    [NotNull]
    public string? DeviceName { get; init; }

    /// <summary>
    /// 标记组名称。
    /// </summary>
    /// <remarks>若标记隶属于设备，那么标记组可为空。</remarks>
    public string? TagGroupName { get; init; }
}
