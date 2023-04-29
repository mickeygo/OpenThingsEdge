using Ops.Communication.Core;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 连接状态
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// 初始化，等待连接
    /// </summary>
    Wait = 1,

    /// <summary>
    /// 已连接。
    /// </summary>
    Connected,

    /// <summary>
    /// 已与服务断开连接。
    /// </summary>
    Disconnected,

    /// <summary>
    /// 连接终止，表示不会再连接。
    /// </summary>
    Aborted,
}

/// <summary>
/// 驱动状态
/// </summary>
public enum DriverStatus
{
    /// <summary>
    /// 可正常通信
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 驱动挂起中
    /// </summary>
    Suspended = 2,
}

/// <summary>
/// 驱动连接器。
/// </summary>
public sealed class DriverConnector
{
    /// <summary>
    /// 连接ID, 与设备ID一致。
    /// </summary>
    [NotNull]
    public string? Id { get; }

    [NotNull]
    public string? Host { get; }

    public int Port { get; }

    /// <summary>
    /// 连接驱动
    /// </summary>
    [NotNull]
    public IReadWriteNet? Driver { get; }

    /// <summary>
    /// 连接处于的状态
    /// </summary>
    public ConnectionStatus ConnectedStatus { get; set; } = ConnectionStatus.Wait;

    /// <summary>
    /// 表示可与服务器进行连接（能 Ping 通）。
    /// </summary>
    public bool Available { get; internal set; }

    /// <summary>
    /// 驱动状态
    /// </summary>
    public DriverStatus DriverStatus { get; internal set; } = DriverStatus.Normal;

    public DriverConnector(string id, string host, int port, IReadWriteNet driver)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
    }

    /// <summary>
    /// 是否可连接
    /// </summary>
    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;
}
