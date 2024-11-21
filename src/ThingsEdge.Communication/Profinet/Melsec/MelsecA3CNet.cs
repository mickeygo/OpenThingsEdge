using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址。
/// </summary>
public class MelsecA3CNet : DeviceSerialPort, IReadWriteA3C, IReadWriteDevice, IReadWriteNet
{
    public byte Station { get; set; }

    public bool SumCheck { get; set; } = true;

    public int Format { get; set; } = 1;

    public bool EnableWriteBitToWordRegister { get; set; }

    public MelsecA3CNet()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
    }

    public async Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return await MelsecA3CNetHelper.ReadPlcTypeAsync(this).ConfigureAwait(false);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return MelsecA3CNetHelper.ReadAsync(this, address, length);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return MelsecA3CNetHelper.ReadBoolAsync(this, address, length);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return MelsecA3CNetHelper.WriteAsync(this, address, data);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return MelsecA3CNetHelper.WriteAsync(this, address, values);
    }

    public Task<OperateResult> RemoteRunAsync()
    {
        return MelsecA3CNetHelper.RemoteRunAsync(this);
    }

    public Task<OperateResult> RemoteStopAsync()
    {
        return MelsecA3CNetHelper.RemoteStopAsync(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecA3CNet[{PortName}:{BaudRate}]";
    }
}
