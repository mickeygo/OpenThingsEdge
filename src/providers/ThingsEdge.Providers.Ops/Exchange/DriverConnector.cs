using Ops.Communication.Core;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动连接器。
/// </summary>
public sealed class DriverConnector : IDriverConnector
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
    public bool Available { get; set; }

    /// <summary>
    /// 驱动状态
    /// </summary>
    public DriverStatus DriverStatus { get; set; } = DriverStatus.Normal;

    public DriverConnector(string id, string host, int port, IReadWriteNet driver)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
    }

    /// <summary>
    /// 是否可连接。
    /// </summary>
    /// <remarks></remarks>
    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;
}
