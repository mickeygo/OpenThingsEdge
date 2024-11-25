using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC通讯类，采用Fins-Tcp通信协议实现，支持的地址信息参见api文档信息。本协议下PLC默认的端口号为 9600，也可以手动更改，重启PLC更改生效。
/// </summary>
/// <remarks>
/// <note type="important">PLC的IP地址的要求，最后一个整数的范围应该小于250，否则会发生连接不上的情况。</note>
/// <note type="warning">如果在测试的时候报错误码64，经网友 上海-Lex 指点，是因为PLC中产生了报警，如伺服报警，模块错误等产生的，但是数据还是能正常读到的，屏蔽64报警或清除plc错误可解决</note>
/// <note type="warning">如果碰到NX系列连接失败，或是无法读取的，需要使用网口2，配置ip地址，网线连接网口2，配置FINSTCP，把UDP的端口改成9601的，这样就可以读写了。</note>
/// 需要特别注意 <see cref="ReadSplits" /> 属性，在超长数据读取时，规定了切割读取的长度，在不是CP1H及扩展模块的时候，可以设置为999，提高一倍的通信速度。
/// </remarks>
public class OmronFinsNet : DeviceTcpNet, IOmronFins, IReadWriteDevice, IReadWriteNet
{
    private readonly byte[] _handSingle =
    [
        70, 73, 78, 83, 0, 0, 0, 12, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ];

    private readonly IncrementCounter _counter = new(255L, 0L);

    /// <summary>
    /// 信息控制字段，默认0x80。
    /// </summary>
    public byte ICF { get; set; } = 128;

    /// <summary>
    /// 系统使用的内部信息。
    /// </summary>
    public byte RSV { get; private set; }

    /// <summary>
    /// 网络层信息，默认0x02，如果有八层消息，就设置为0x07。
    /// </summary>
    public byte GCT { get; set; } = 2;

    /// <summary>
    /// PLC的网络号地址，默认0x00。
    /// </summary>
    /// <remarks>
    /// 00: Local network<br />
    /// 01-7F: Remote network address (decimal: 1 to 127)
    /// </remarks>
    public byte DNA { get; set; }

    /// <summary>
    /// PLC的节点地址，默认为0，在和PLC连接的过程中，自动从PLC获取到DA1的值。
    /// </summary>
    public byte DA1 { get; set; }

    /// <summary>
    /// PLC的单元号地址，通常都为0。
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///   <item>00: CPU Unit</item>
    ///   <item>FE: Controller Link Unit or Ethernet Unit connected to network</item>
    ///   <item>10 TO 1F: CPU Bus Unit</item>
    ///   <item>E1: Inner Board</item>
    /// </list>
    /// </remarks>
    public byte DA2 { get; set; }

    /// <summary>
    /// 上位机的网络号地址。
    /// </summary>
    /// <remarks>
    /// 00: Local network<br />
    /// 01-7F: Remote network (1 to 127 decimal)
    /// </remarks>
    public byte SNA { get; set; }

    /// <summary>
    /// 上位机的节点地址，默认是0x01，当连接PLC之后，将由PLC来设定当前的值。
    /// </summary>
    /// <remarks>
    /// v9.6.5版本及之前的版本都需要手动设置，如果是多连接，相同的节点是连接不上PLC的。
    /// </remarks>
    public byte SA1 { get; set; } = 1;

    /// <summary>
    /// 上位机的单元号地址。
    /// </summary>
    /// <remarks>
    /// 00: CPU Unit<br />
    /// 10-1F: CPU Bus Unit
    /// </remarks>
    public byte SA2 { get; set; }

    /// <summary>
    /// 服务的标识号，由客户端生成自增的顺序值，用来标识和校验通信报文的ID。
    /// </summary>
    public byte SID { get; set; }

    /// <summary>
    /// 进行字读取的时候对于超长的情况按照本属性进行切割，默认500，如果不是CP1H及扩展模块的，可以设置为999，可以提高一倍的通信速度。
    /// </summary>
    public int ReadSplits { get; set; } = 500;

    /// <summary>
    /// 当接收PLC返回的数据的时候，获取或设置是否需要接收数据直到空为止，在一些及其特殊的场景里，可以设置为 true 防止数据错误的情况。
    /// </summary>
    public bool ReceiveUntilEmpty { get; set; }

    /// <inheritdoc cref="IOmronFins.PlcType" />
    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;

    /// <summary>
    /// 指定ip地址和端口号来实例化一个欧姆龙PLC Fins帧协议的通讯对象。
    /// </summary>
    /// <param name="ipAddress">PLCd的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public OmronFinsNet(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB)
        {
            IsStringReverseByteWord = true,
        };
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FinsMessage();
    }

    /// <summary>
    /// 将普通的指令打包成完整的指令。
    /// </summary>
    /// <param name="cmd">FINS的核心指令</param>
    /// <returns>完整的可用于发送PLC的命令</returns>
    private byte[] PackCommand(byte[] cmd)
    {
        var array = new byte[26 + cmd.Length];
        Array.Copy(_handSingle, 0, array, 0, 4);
        var bytes = BitConverter.GetBytes(array.Length - 8);
        Array.Reverse(bytes);
        bytes.CopyTo(array, 4);
        array[11] = 2;
        array[16] = ICF;
        array[17] = RSV;
        array[18] = GCT;
        array[19] = DNA;
        array[20] = DA1;
        array[21] = DA2;
        array[22] = SNA;
        array[23] = SA1;
        array[24] = SA2;
        array[25] = (byte)_counter.OnNext();
        cmd.CopyTo(array, 26);
        SID = array[25];
        return array;
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(NetworkPipe, _handSingle, hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }

        var status = BitConverter.ToInt32(
        [
            read.Content[15],
            read.Content[14],
            read.Content[13],
            read.Content[12]
        ], 0);
        if (status != 0)
        {
            return new OperateResult(status, OmronFinsNetHelper.GetStatusDescription(status));
        }
        if (read.Content.Length >= 20)
        {
            SA1 = read.Content[19];
        }
        if (read.Content.Length >= 24)
        {
            DA1 = read.Content[23];
        }
        _counter.Reset();
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return PackCommand(command);
    }

    /// <inheritdoc />
    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronFinsNetHelper.ResponseValidAnalysis(response);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await OmronFinsNetHelper.ReadAsync(this, address, length, ReadSplits).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await OmronFinsNetHelper.WriteAsync(this, address, data).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await base.ReadStringAsync(address, length, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await base.WriteAsync(address, value, Encoding.UTF8).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadAsync(string[] address)
    {
        return await OmronFinsNetHelper.ReadAsync(this, address).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await OmronFinsNetHelper.ReadBoolAsync(this, address, length, ReadSplits).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await OmronFinsNetHelper.WriteAsync(this, address, values).ConfigureAwait(false);
    }

    public async Task<OperateResult> RunAsync()
    {
        return await OmronFinsNetHelper.RunAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<OperateResult> StopAsync()
    {
        return await OmronFinsNetHelper.StopAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync()
    {
        return await OmronFinsNetHelper.ReadCpuUnitDataAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync()
    {
        return await OmronFinsNetHelper.ReadCpuUnitStatusAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<OperateResult<DateTime>> ReadCpuTimeAsync()
    {
        return await OmronFinsNetHelper.ReadCpuTimeAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronFinsNet[{IpAddress}:{Port}]";
    }
}
