namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// <see cref="DeviceTcpNet"/> 创建选项。
/// </summary>
public sealed class DeviceTcpNetOptions
{
    /// <summary>
    /// Socket 连接池最大数量。
    /// </summary>
    public int SocketPoolSize { get; set; }

    /// <summary>
    /// 连接超时时间，单位 ms。
    /// </summary>
    public int ConnectTimeout { get; set; }

    /// <summary>
    /// 保活时长，单位 ms。
    /// </summary>
    public int KeepAliveTime { get; set; }
}
