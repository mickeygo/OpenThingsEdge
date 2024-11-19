using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的HostLink的C-Mode实现形式，地址支持携带站号信息，例如：s=2;D100。
/// </summary>
/// <remarks>
/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
/// </remarks>
public class OmronHostLinkCMode : DeviceSerialPort, IHostLinkCMode, IReadWriteNet
{
    /// <summary>
    /// PLC设备的站号信息。
    /// </summary>
    public byte UnitNumber { get; set; }

    public OmronHostLinkCMode()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        WordLength = 1;
        ByteTransform.IsStringReverseByteWord = true;
        ReceiveEmptyDataCount = 5;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13);
    }
    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return OmronHostLinkCModeHelper.ReadAsync(this, UnitNumber, address, length);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        // TODO: [NotImplemented] OmronHostLinkCMode -> ReadBoolAsync
        throw new NotImplementedException();
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        return OmronHostLinkCModeHelper.WriteAsync(this, UnitNumber, address, values);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] OmronHostLinkCMode -> WriteAsync
        throw new NotImplementedException();
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return ReadPlcTypeAsync(UnitNumber);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync(byte unitNumber)
    {
        return OmronHostLinkCModeHelper.ReadPlcTypeAsync(this, unitNumber);
    }

    public Task<OperateResult<int>> ReadPlcModeAsync()
    {
        return ReadPlcModeAsync(UnitNumber);
    }

    public Task<OperateResult<int>> ReadPlcModeAsync(byte unitNumber)
    {
        return OmronHostLinkCModeHelper.ReadPlcModeAsync(this, unitNumber);
    }

    public Task<OperateResult> ChangePlcMode(byte mode)
    {
        return ChangePlcModeAsync(UnitNumber, mode);
    }

    public Task<OperateResult> ChangePlcModeAsync(byte unitNumber, byte mode)
    {
        return OmronHostLinkCModeHelper.ChangePlcModeAsync(this, unitNumber, mode);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronHostLinkCMode[{PortName}:{BaudRate}]";
    }
}
