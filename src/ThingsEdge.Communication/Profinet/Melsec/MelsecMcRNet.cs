using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱的R系列的MC协议，支持的地址类型和 <see cref="MelsecMcNet" /> 有区别，详细请查看对应的相关文档说明。
/// </summary>
public class MelsecMcRNet : DeviceTcpNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    public McType McType => McType.McRBinary;

    public byte NetworkNumber { get; set; }

    public byte NetworkStationNumber { get; set; }

    public byte PLCNumber { get; set; } = byte.MaxValue;

    public bool EnableWriteBitToWordRegister { get; set; }

    public ushort TargetIOStation { get; set; } = 1023;

    /// <summary>
    /// 指定ip地址和端口号来实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public MelsecMcRNet(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecQnA3EBinaryMessage();
    }

    /// <inheritdoc cref="IReadWriteMc.McAnalysisAddress" />
    public virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit)
    {
        return McAddressData.ParseMelsecRFrom(address, length, isBit);
    }

    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return McBinaryHelper.PackMcCommand(this, command);
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var operateResult = McBinaryHelper.CheckResponseContentHelper(response);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(11));
    }

    /// <inheritdoc cref="IReadWriteMc.ExtractActualData" />
    public byte[] ExtractActualData(byte[] response, bool isBit)
    {
        return McBinaryHelper.ExtractActualDataHelper(response, isBit);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await McHelper.ReadAsync(this, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await McHelper.ReadBoolAsync(this, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await McHelper.WriteAsync(this, address, data).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await McHelper.WriteAsync(this, address, values).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
    {
        return await McHelper.ReadRandomAsync(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address, ushort[] length)
    {
        return await McHelper.ReadRandomAsync(this, address, length).ConfigureAwait(false);
    }

    public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
    {
        return await McHelper.ReadRandomInt16Async(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult<ushort[]>> ReadRandomUInt16Async(string[] address)
    {
        return await McHelper.ReadRandomUInt16Async(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult> RemoteRunAsync()
    {
        return await McHelper.RemoteRunAsync(this).ConfigureAwait(false);
    }

    public async Task<OperateResult> RemoteStopAsync()
    {
        return await McHelper.RemoteStopAsync(this).ConfigureAwait(false);
    }

    public async Task<OperateResult> RemoteResetAsync()
    {
        return await McHelper.RemoteResetAsync(this).ConfigureAwait(false);
    }

    public async Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return await McHelper.ReadPlcTypeAsync(this).ConfigureAwait(false);
    }

    public async Task<OperateResult> ErrorStateResetAsync()
    {
        return await McHelper.ErrorStateResetAsync(this).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"MelsecMcRNet[{IpAddress}:{Port}]";
    }
}
