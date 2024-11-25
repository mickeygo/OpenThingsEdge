using ThingsEdge.Communication.Core;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 驱动连接器。
/// </summary>
internal interface IDriverConnector
{
    /// <summary>
    /// 连接ID, 与设备ID一致。
    /// </summary>
    [NotNull]
    string? Id { get; }

    [NotNull]
    string? Host { get; }

    int Port { get; }

    /// <summary>
    /// 允许一次性读取的最大 PDU 长度（byte数量），0 表示不指定。
    /// </summary>
    /// <remarks>
    /// 西门子 S7 协议批量读取时 PDU 超过最大长度会出现异常，这时可指定最大长度，一次批量读取时内部按最大长度切分到多次读取。
    /// </remarks>
    int MaxPDUSize { get; }

    /// <summary>
    /// 连接驱动
    /// </summary>
    [NotNull]
    IReadWriteNet? Driver { get; }

    /// <summary>
    /// 连接处于的状态
    /// </summary>
    ConnectionStatus ConnectedStatus { get; set; }

    /// <summary>
    /// 表示可与服务器进行连接（能 Ping 通）。
    /// </summary>
    bool Available { get; set; }

    /// <summary>
    /// 驱动状态
    /// </summary>
    DriverStatus DriverStatus { get; set; }

    /// <summary>
    /// 是否可连接。
    /// </summary>
    /// <remarks></remarks>
    bool CanConnect { get; }
}

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
    /// 驱动连接挂起中
    /// </summary>
    Suspended = 2,
}
