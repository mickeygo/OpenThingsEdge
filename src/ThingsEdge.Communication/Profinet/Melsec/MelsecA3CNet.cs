using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址<br />
/// Based on Qna-compatible 3C frame format one communication, the specific address needs to refer to the basic address of Mitsubishi.
/// </summary>
/// <remarks>
/// <inheritdoc cref="T:HslCommunication.Profinet.Melsec.MelsecA3CNetOverTcp" path="remarks" />
/// </remarks>
/// <example>
/// <inheritdoc cref="T:HslCommunication.Profinet.Melsec.MelsecA3CNetOverTcp" path="example" />
/// </example>
public class MelsecA3CNet : DeviceSerialPort, IReadWriteA3C, IReadWriteDevice, IReadWriteNet
{
    private byte station = 0;

    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C.Station" />
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

    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C.SumCheck" />
    public bool SumCheck { get; set; } = true;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C.Format" />
    public int Format { get; set; } = 1;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.EnableWriteBitToWordRegister" />
    public bool EnableWriteBitToWordRegister { get; set; }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecA3CNetOverTcp.#ctor" />
    public MelsecA3CNet()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.Read(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C,System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return MelsecA3CNetHelper.Read(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.Write(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C,System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return MelsecA3CNetHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C,System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return MelsecA3CNetHelper.ReadBool(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecA3CNetOverTcp.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] value)
    {
        return MelsecA3CNetHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.RemoteRun(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C)" />
    [HslMqttApi]
    public OperateResult RemoteRun()
    {
        return MelsecA3CNetHelper.RemoteRun(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.RemoteStop(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C)" />
    [HslMqttApi]
    public OperateResult RemoteStop()
    {
        return MelsecA3CNetHelper.RemoteStop(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecA3CNetHelper.ReadPlcType(HslCommunication.Profinet.Melsec.Helper.IReadWriteA3C)" />
    [HslMqttApi]
    public OperateResult<string> ReadPlcType()
    {
        return MelsecA3CNetHelper.ReadPlcType(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecA3CNet[{PortName}:{BaudRate}]";
    }
}
