using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.ModBus;

/// <summary>
/// Modbus-Ascii通讯协议的网口透传类，基于rtu类库完善过来，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明。
/// </summary>
public class ModbusAsciiOverTcp : ModbusRtuOverTcp
{
    /// <summary>
    /// 实例化一个Modbus-ascii协议的客户端对象。
    /// </summary>
    public ModbusAsciiOverTcp()
    {
    }

    public ModbusAsciiOverTcp(string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13, 10);
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.TransModbusCoreToAsciiPackCommand(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return ModbusHelper.ExtraAsciiResponseContent(send, response, BroadcastStation);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ModbusAsciiOverTcp[{IpAddress}:{Port}]";
    }
}
