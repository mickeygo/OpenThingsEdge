using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Panasonic.Helper;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，基于Tcp透传实现的机制。
/// </summary>
/// <remarks>
/// 地址支持携带站号的访问方式，例如：s=2;D100。
/// </remarks>
public class PanasonicMewtocolOverTcp : DeviceTcpNet
{
    /// <summary>
    /// PLC设备的目标站号，需要根据实际的设置来填写。
    /// </summary>
    public byte Station { get; set; }

    /// <summary>
    /// 实例化一个默认的松下PLC通信对象，指定ip地址，端口，默认站号为0xEE。
    /// </summary>
    /// <param name="ipAddress">Ip地址数据</param>
    /// <param name="port">端口号</param>
    /// <param name="station">站号信息，默认为0xEE</param>
    public PanasonicMewtocolOverTcp(string ipAddress, int port, byte station = 238) : base(ipAddress, port)
    {
        ByteTransform = new RegularByteTransform
        {
            DataFormat = DataFormat.DCBA,
        };
        WordLength = 1;
        Station = station;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return MewtocolHelper.ReadPlcTypeAsync(this, Station);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return MewtocolHelper.ReadAsync(this, Station, address, length);
    }

    public override Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return MewtocolHelper.ReadBoolAsync(this, Station, address);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return MewtocolHelper.ReadBoolAsync(this, Station, address, length);
    }

    public Task<OperateResult<bool[]>> ReadBoolAsync(string[] addresses)
    {
        return MewtocolHelper.ReadBoolAsync(this, Station, addresses);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return MewtocolHelper.WriteAsync(this, Station, address, data);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return MewtocolHelper.WriteAsync(this, Station, address, values);
    }

    public override Task<OperateResult> WriteAsync(string address, bool value)
    {
        return MewtocolHelper.WriteAsync(this, Station, address, value);
    }

    public Task<OperateResult> WriteAsync(string[] addresses, bool[] values)
    {
        return MewtocolHelper.WriteAsync(this, Station, addresses, values);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PanasonicMewtocolOverTcp[{IpAddress}:{Port}]";
    }
}
