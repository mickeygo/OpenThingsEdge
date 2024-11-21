using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯。
/// </summary>
/// <remarks>
/// 支持读写的数据类型详细参考API文档，支持高级的数据读取，例如读取智能模块，缓冲存储器等等。如果使用的Fx5u的PLC，如果之前有写过用户认证，需要对设备信息全部初始化。
/// </remarks>
public class MelsecMcNet : DeviceTcpNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    public virtual McType McType => McType.McBinary;

    public byte NetworkNumber { get; set; }

    public byte PLCNumber { get; set; } = byte.MaxValue;

    public byte NetworkStationNumber { get; set; }

    public bool EnableWriteBitToWordRegister { get; set; }

    /// <inheritdoc cref="IReadWriteMc.TargetIOStation" />
    public ushort TargetIOStation { get; set; } = 1023;

    /// <summary>
    /// 指定ip地址和端口号来实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public MelsecMcNet(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecQnA3EBinaryMessage();
    }

    public virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit)
    {
        return McAddressData.ParseMelsecFrom(address, length, isBit);
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

    public virtual byte[] ExtractActualData(byte[] response, bool isBit)
    {
        return McBinaryHelper.ExtractActualDataHelper(response, isBit);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await McHelper.ReadAsync(this, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await McHelper.WriteAsync(this, address, data).ConfigureAwait(false);
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

    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return await base.ReadBoolAsync(address).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await McHelper.ReadBoolAsync(this, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await McHelper.WriteAsync(this, address, values).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadTagsAsync(string tag, ushort length)
    {
        return await ReadTagsAsync([tag], [length]).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadTagsAsync(string[] tags, ushort[] length)
    {
        return await McBinaryHelper.ReadTagsAsync(this, tags, length).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadExtendAsync(ushort extend, string address, ushort length)
    {
        return await McHelper.ReadExtendAsync(this, extend, address, length).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadMemoryAsync(string address, ushort length)
    {
        return await McHelper.ReadMemoryAsync(this, address, length).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadSmartModuleAsync(ushort module, string address, ushort length)
    {
        return await McHelper.ReadSmartModuleAsync(this, module, address, length).ConfigureAwait(false);
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
        return $"MelsecMcNet[{IpAddress}:{Port}]";
    }
}
