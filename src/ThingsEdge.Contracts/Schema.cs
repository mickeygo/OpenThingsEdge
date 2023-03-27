namespace ThingsEdge.Contracts;

public sealed class Schema
{
    /// <summary>
    /// 线体编号
    /// </summary>
    [NotNull]
    public string? Line { get; set; }

    /// <summary>
    /// 线体名称
    /// </summary>
    [NotNull]
    public string? LineName { get; set; }

    /// <summary>
    /// 工站编号，每条产线工站编号唯一。
    /// </summary>
    [NotNull]
    public string? Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    [NotNull]
    public string? StationName { get; set; }

    /// <summary>
    /// 设备主机（IP地址）。
    /// </summary>
    [NotNull]
    public string? Host { get; set; }

    /// <summary>
    /// 设备网络端口，0 表示会按驱动默认端口设置。
    /// </summary>
    public int Port { get; set; }
}
