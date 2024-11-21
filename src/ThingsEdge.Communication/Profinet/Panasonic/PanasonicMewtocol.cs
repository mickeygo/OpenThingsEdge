using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Panasonic.Helper;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，支持的地址列表参考api文档。
/// </summary>
/// <remarks>
/// 地址支持携带站号的访问方式，例如：s=2;D100。
/// </remarks>
public class PanasonicMewtocol : DeviceSerialPort
{
    public byte Station { get; set; }

    public PanasonicMewtocol(byte station = 238)
    {
        ByteTransform = new RegularByteTransform();
        Station = station;
        ByteTransform.DataFormat = DataFormat.DCBA;
        WordLength = 1;
        ReceiveEmptyDataCount = 5;
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

    public override Task<OperateResult> WriteAsync(string address, bool value)
    {
        return MewtocolHelper.WriteAsync(this, Station, address, value);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return MewtocolHelper.WriteAsync(this, Station, address, values);
    }

    public Task<OperateResult> WriteAsync(string[] addresses, bool[] values)
    {
        return MewtocolHelper.WriteAsync(this, Station, addresses, values);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PanasonicMewtocol[{PortName}:{BaudRate}]";
    }
}
