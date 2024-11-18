using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士PLC的SPB协议，详细的地址信息见api文档说明，地址可以携带站号信息，例如：s=2;D100，PLC侧需要配置无BCC计算，包含0D0A结束码<br />
/// Fuji PLC's SPB protocol. For detailed address information, see the api documentation, 
/// The address can carry station number information, for example: s=2;D100, PLC side needs to be configured with no BCC calculation, including 0D0A end code
/// </summary>
public class FujiSPBOverTcp : DeviceTcpNet
{
    private byte station = 1;

    /// <summary>
    /// PLC的站号信息<br />
    /// PLC station number information
    /// </summary>
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

    /// <summary>
    /// 使用默认的构造方法实例化对象<br />
    /// Instantiate the object using the default constructor
    /// </summary>
    public FujiSPBOverTcp()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
        SleepTime = 20;
    }

    /// <summary>
    /// 使用指定的ip地址和端口来实例化一个对象<br />
    /// Instantiate an object with the specified IP address and port
    /// </summary>
    /// <param name="ipAddress">设备的Ip地址</param>
    /// <param name="port">设备的端口号</param>
    public FujiSPBOverTcp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FujiSPBMessage();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Read(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return FujiSPBHelper.Read(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return FujiSPBHelper.Write(this, station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return FujiSPBHelper.ReadBool(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean)" />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        return FujiSPBHelper.Write(this, station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadAsync(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await FujiSPBHelper.WriteAsync(this, station, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadBoolAsync(this, station, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBOverTcp.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await FujiSPBHelper.WriteAsync(this, station, address, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FujiSPBOverTcp[{IpAddress}:{Port}]";
    }
}
