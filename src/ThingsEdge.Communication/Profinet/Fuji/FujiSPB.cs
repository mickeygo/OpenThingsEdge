using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士PLC的SPB协议，详细的地址信息见api文档说明，地址可以携带站号信息，例如：s=2;D100，PLC侧需要配置无BCC计算，包含0D0A结束码<br />
/// Fuji PLC's SPB protocol. For detailed address information, see the api documentation, 
/// The address can carry station number information, for example: s=2;D100, PLC side needs to be configured with no BCC calculation, including 0D0A end code
/// </summary>
public class FujiSPB : DeviceSerialPort
{
    private byte station = 1;

    /// <inheritdoc cref="P:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Station" />
    public byte Station
    {
        get
        {
            return station;
        }
        set
        {
            station = value;
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.#ctor" />
    public FujiSPB()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
        ReceiveEmptyDataCount = 5;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FujiSPBMessage();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return FujiSPBHelper.Read(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return FujiSPBHelper.Write(this, station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.ReadBool(System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return FujiSPBHelper.ReadBool(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Write(System.String,System.Boolean)" />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        return FujiSPBHelper.Write(this, station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPB.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await FujiSPBHelper.WriteAsync(this, station, address, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FujiSPB[{PortName}:{BaudRate}]";
    }
}
