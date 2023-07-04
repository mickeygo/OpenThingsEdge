using Ops.Communication.Core;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 驱动连接器。
/// </summary>
public sealed class DriverConnector : IDriverConnector
{
    [NotNull]
    public string? Id { get; }

    [NotNull]
    public string? Host { get; }

    public int Port { get; }

    [NotNull]
    public IReadWriteNet? Driver { get; }

    public ConnectionStatus ConnectedStatus { get; set; } = ConnectionStatus.Wait;

    public bool Available { get; set; }

    public DriverStatus DriverStatus { get; set; } = DriverStatus.Normal;

    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;

    public long ErrorCount { get; set; }

    public DriverConnector(string id, string host, int port, IReadWriteNet driver)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
    }
}
