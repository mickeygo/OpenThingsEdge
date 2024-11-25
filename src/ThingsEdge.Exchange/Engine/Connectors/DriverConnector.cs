using ThingsEdge.Communication.Core;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 驱动连接器。
/// </summary>
internal sealed class DriverConnector : IDriverConnector
{
    [NotNull]
    public string? Id { get; }

    [NotNull]
    public string? Host { get; }

    public int Port { get; }

    public int MaxPDUSize { get; }

    [NotNull]
    public IReadWriteNet? Driver { get; }

    public ConnectionStatus ConnectedStatus { get; set; } = ConnectionStatus.Wait;

    public bool Available { get; set; }

    public DriverStatus DriverStatus { get; set; } = DriverStatus.Normal;

    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;

    public DriverConnector(string id, string host, int port, IReadWriteNet driver, int maxPDUSize)
    {
        Id = id;
        Host = host;
        Port = port;
        Driver = driver;
        MaxPDUSize = maxPDUSize;
    }
}
