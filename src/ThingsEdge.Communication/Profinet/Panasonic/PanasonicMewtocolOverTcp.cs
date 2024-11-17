using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Panasonic.Helper;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，基于Tcp透传实现的机制，支持的地址列表参考api文档<br />
/// The data exchange protocol of Panasonic PLC adopts Mewtocol protocol for communication. 
/// It is based on the mechanism of Tcp transparent transmission. For the list of supported addresses, refer to the api document.
/// </summary>
/// <remarks>
/// 地址支持携带站号的访问方式，例如：s=2;D100
/// </remarks>
public class PanasonicMewtocolOverTcp : DeviceTcpNet
{
    /// <summary>
    /// PLC设备的目标站号，需要根据实际的设置来填写<br />
    /// The target station number of the PLC device needs to be filled in according to the actual settings
    /// </summary>
    public byte Station { get; set; }

    /// <summary>
    /// 实例化一个默认的松下PLC通信对象，默认站号为0xEE<br />
    /// Instantiate a default Panasonic PLC communication object, the default station number is 0xEE
    /// </summary>
    /// <param name="station">站号信息，默认为0xEE</param>
    public PanasonicMewtocolOverTcp(byte station = 238)
    {
        ByteTransform = new RegularByteTransform();
        Station = station;
        ByteTransform.DataFormat = DataFormat.DCBA;
        WordLength = 1;
    }

    /// <summary>
    /// 实例化一个默认的松下PLC通信对象，指定ip地址，端口，默认站号为0xEE<br />
    /// Instantiate a default Panasonic PLC communication object, specify the IP address, port, and the default station number is 0xEE
    /// </summary>
    /// <param name="ipAddress">Ip地址数据</param>
    /// <param name="port">端口号</param>
    /// <param name="station">站号信息，默认为0xEE</param>
    public PanasonicMewtocolOverTcp(string ipAddress, int port, byte station = 238)
        : this(station)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Read(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return MewtocolHelper.Read(this, Station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return MewtocolHelper.Write(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await MewtocolHelper.ReadAsync(this, Station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await MewtocolHelper.WriteAsync(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return MewtocolHelper.ReadBool(this, Station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String)" />
    [HslMqttApi("ReadBool", "")]
    public override OperateResult<bool> ReadBool(string address)
    {
        return MewtocolHelper.ReadBool(this, Station, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String[])" />
    public OperateResult<bool[]> ReadBool(string[] address)
    {
        return MewtocolHelper.ReadBool(this, Station, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return MewtocolHelper.Write(this, Station, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean)" />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        return MewtocolHelper.Write(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String[],System.Boolean[])" />
    public OperateResult Write(string[] address, bool[] value)
    {
        return MewtocolHelper.Write(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.ReadBool(System.String)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await MewtocolHelper.ReadBoolAsync(this, Station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.ReadBool(System.String)" />
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return await MewtocolHelper.ReadBoolAsync(this, Station, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await MewtocolHelper.WriteAsync(this, Station, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await MewtocolHelper.WriteAsync(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.ReadBool(System.String[])" />
    public async Task<OperateResult<bool[]>> ReadBoolAsync(string[] address)
    {
        return await MewtocolHelper.ReadBoolAsync(this, Station, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.PanasonicMewtocolOverTcp.Write(System.String[],System.Boolean[])" />
    public async Task<OperateResult> WriteAsync(string[] address, bool[] value)
    {
        return await MewtocolHelper.WriteAsync(this, Station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.Byte)" />
    public OperateResult<string> ReadPlcType()
    {
        return MewtocolHelper.ReadPlcType(this, Station);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PanasonicMewtocolOverTcp[{IpAddress}:{Port}]";
    }
}
