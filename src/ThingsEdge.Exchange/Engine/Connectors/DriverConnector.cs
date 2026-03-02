using ThingsEdge.Communication.Core;

namespace ThingsEdge.Exchange.Engine.Connectors;

/// <summary>
/// 驱动连接器。
/// </summary>
internal sealed class DriverConnector(string id, string host, int port, IReadWriteNet driver) : IDriverConnector
{
    [NotNull]
    public string? Id { get; } = id;

    [NotNull]
    public string? Host { get; } = host;

    public int Port { get; } = port;

    [NotNull]
    public IReadWriteNet? Driver { get; } = driver;

    public ConnectionStatus ConnectedStatus { get; set; } = ConnectionStatus.Wait;

    public bool Available { get; set; }

    public DriverStatus DriverStatus { get; set; } = DriverStatus.Normal;

    public bool CanConnect => Available && DriverStatus == DriverStatus.Normal && ConnectedStatus == ConnectionStatus.Connected;
}
