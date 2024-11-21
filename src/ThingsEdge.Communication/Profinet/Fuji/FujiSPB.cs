using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士PLC的SPB协议，详细的地址信息见api文档说明，地址可以携带站号信息，例如：s=2;D100，PLC侧需要配置无BCC计算，包含0D0A结束码。
/// </summary>
public class FujiSPB : DeviceSerialPort
{
    public byte Station { get; set; } = 1;

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

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadAsync(this, Station, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadBoolAsync(this, Station, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await FujiSPBHelper.WriteAsync(this, Station, address, data).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await FujiSPBHelper.WriteAsync(this, Station, address, value).ConfigureAwait(false);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] FujiSPB -> WriteAsync

        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FujiSPB[{PortName}:{BaudRate}]";
    }

}
