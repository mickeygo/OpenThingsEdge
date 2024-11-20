using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.GE;

/// <summary>
/// Ge(通用电气)集团的SRTP协议实现的客户端，支持 I,Q,M,T,SA,SB,SC,S,G 的位和字节读写，支持 AI,AQ,R 的字读写操作，支持读取PLC时间，程序名操作。
/// </summary>
/// <remarks>
/// PLC的端口号默认18245，其中读取R，AI，AQ寄存器的原始字节时，传入的长度参数为字节长度。
/// 对其他寄存器而言，M1-M8的位读取，相当于 M1的字节读取。写入也是同理。
/// </remarks>

public class GeSRTPNet : DeviceTcpNet
{
    private readonly SoftIncrementCount _incrementCount = new(65535L, 0L);

    /// <summary>
    /// 指定IP地址和端口号来实例化一个对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public GeSRTPNet(string ipAddress, int port = 18245) : base(ipAddress, port)
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 2;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new GeSRTPMessage();
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(Pipe, new byte[56], hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度，返回原始的字节数组，支持 I,Q,M,T,SA,SB,SC,S,G 的位和字节读写，支持 AI,AQ,R 的字读写操作，地址示例：R1,M1。
    /// </summary>
    /// <remarks>
    /// 其中读取R，AI，AQ寄存器的原始字节时，传入的长度参数为字节长度。长度为10，返回10个字节数组信息，如果返回长度不满6个字节的，一律返回6个字节的数据
    /// </remarks>
    /// <param name="address">数据地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var build = GeHelper.BuildReadCommand(_incrementCount.GetCurrentValue(), address, length, isBit: false);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <summary>
    /// 根据指定的地址来读取一个字节的数据，按照字节为单位，例如 M1 字节，就是指 M1-M8 位组成的字节，M2 字节就是 M9-M16 组成的字节。不支持对 AI,AQ,R 寄存器的字节读取。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的 <see cref="byte" /> 数据</returns>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        var analysis = GeSRTPAddress.ParseFrom(address, 1, isBit: true);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(analysis);
        }
        if (analysis.Content.DataCode == 10 || analysis.Content.DataCode == 12 || analysis.Content.DataCode == 8)
        {
            return new OperateResult<byte>(StringResources.Language.GeSRTPNotSupportByteReadWrite);
        }
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }

    /// <summary>
    /// 按照位为单位，批量从指定的地址里读取 bool 数组数据，不支持 AI，AQ，R 地址类型，地址比如从1开始，例如 I1,Q1,M1,T1,SA1,SB1,SC1,S1,G1。
    /// </summary>
    /// <param name="address">PLC的地址信息，例如 M1, G1</param>
    /// <param name="length">读取的长度信息，按照位为单位</param>
    /// <returns>包含是否读取成功的bool数组</returns>
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var analysis = GeSRTPAddress.ParseFrom(address, length, isBit: true);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }
        var build = GeHelper.BuildReadCommand(_incrementCount.GetCurrentValue(), analysis.Content);
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        var extra = GeHelper.ExtraResponseContent(read.Content);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(extra);
        }
        return OperateResult.CreateSuccessResult(extra.Content.ToBoolArray().SelectMiddle(analysis.Content.AddressStart % 8, length));
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        var build = GeHelper.BuildWriteCommand(_incrementCount.GetCurrentValue(), address, values);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <summary>
    /// 向PLC中写入byte数据，返回是否写入成功。
    /// </summary>
    /// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
    /// <param name="value">byte数据</param>
    /// <returns>是否写入成功的结果对象</returns>
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        var analysis = GeSRTPAddress.ParseFrom(address, 1, isBit: true);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(analysis);
        }
        if (analysis.Content.DataCode == 10 || analysis.Content.DataCode == 12 || analysis.Content.DataCode == 8)
        {
            return new OperateResult<byte>(StringResources.Language.GeSRTPNotSupportByteReadWrite);
        }
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <summary>
    /// 按照位为单位，批量写入 bool 数组到指定的地址里，不支持 AI，AQ，R 地址类型，地址比如从1开始，例如 I1,Q1,M1,T1,SA1,SB1,SC1,S1,G1。
    /// </summary>
    /// <param name="address">PLC的地址信息，例如 M1, G1</param>
    /// <param name="values">bool 数组</param>
    /// <returns>是否写入成功的结果对象</returns>
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        var build = GeHelper.BuildWriteCommand(_incrementCount.GetCurrentValue(), address, values);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <summary>
    /// 读取PLC当前的时间，这个时间可能是不包含时区的，需要自己转换成本地的时间。
    /// </summary>
    /// <returns>包含是否成功的时间信息</returns>
    public async Task<OperateResult<DateTime>> ReadPLCTimeAsync()
    {
        var build = GeHelper.BuildReadCoreCommand(_incrementCount.GetCurrentValue(), 37, [0, 0, 0, 2, 0]);
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(read);
        }
        var extra = GeHelper.ExtraResponseContent(read.Content);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(extra);
        }
        return GeHelper.ExtraDateTime(extra.Content);
    }

    /// <summary>
    /// 读取PLC当前的程序的名称。
    /// </summary>
    /// <returns>包含是否成的程序名称信息</returns>
    public async Task<OperateResult<string>> ReadProgramNameAsync()
    {
        var build = GeHelper.BuildReadCoreCommand(_incrementCount.GetCurrentValue(), 1, [0, 0, 0, 2, 0]);
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        var extra = GeHelper.ExtraResponseContent(read.Content);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(extra);
        }
        return GeHelper.ExtraProgramName(extra.Content);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"GeSRTPNet[{IpAddress}:{Port}]";
    }
}
