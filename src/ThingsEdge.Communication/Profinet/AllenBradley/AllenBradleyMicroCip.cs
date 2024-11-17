namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的cip通信实现类，适用Micro800系列控制系统。
/// </summary>
public sealed class AllenBradleyMicroCip : AllenBradleyNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.#ctor" />
    public AllenBradleyMicroCip()
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.#ctor(System.String,System.Int32)" />
    public AllenBradleyMicroCip(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <inheritdoc />
    protected override byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
    {
        return AllenBradleyHelper.PackCleanCommandService(portSlot, cips);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AllenBradleyMicroCip[{IpAddress}:{Port}]";
    }
}
