using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.GE;

/// <summary>
/// Ge(通用电气)集团的SRTP协议实现的客户端，支持 I,Q,M,T,SA,SB,SC,S,G 的位和字节读写，支持 AI,AQ,R 的字读写操作，支持读取PLC时间，程序名操作<br />
/// The client implemented by Ge (General Electric) Group's SRTP protocol, supports I, Q, M, T, SA, SB, SC, S, G bit and byte read and write, 
/// supports AI, AQ, R word read and write Operation, support reading PLC time, program name operation
/// </summary>
/// <remarks>
/// PLC的端口号默认18245，其中读取R，AI，AQ寄存器的原始字节时，传入的长度参数为字节长度。<br />
/// 对其他寄存器而言，M1-M8的位读取，相当于 M1的字节读取。写入也是同理。
/// </remarks>
/// <example>
/// <note type="important">对于 AI,AQ,R 类型的数据来说，底层是以字为单位的，但是调用 <see cref="M:HslCommunication.Profinet.GE.GeSRTPNet.Read(System.String,System.UInt16)" /> 来读取原始数据时，需要传入2被的字长度，对于其他方法不影响</note>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\GeSRTPNetSample.cs" region="Usage" title="简单的短连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\GeSRTPNetSample.cs" region="Usage2" title="简单的长连接使用" />
///
/// 假设起始地址为R1，R1存储了温度，100.6℃值为1006，R2存储了压力，1.23Mpa值为123，R3-R4存储了产量计数，读取如下：
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\GeSRTPNetSample.cs" region="ReadExample2" title="Read示例" />
/// 以下是读取不同类型数据的示例
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\GeSRTPNetSample.cs" region="ReadExample1" title="Read示例" />
/// </example>
public class GeSRTPNet : DeviceTcpNet
{
    private SoftIncrementCount incrementCount = new SoftIncrementCount(65535L, 0L);

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public GeSRTPNet()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 2;
    }

    /// <summary>
    /// 指定IP地址和端口号来实例化一个对象<br />
    /// Specify the IP address and port number to instantiate an object
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public GeSRTPNet(string ipAddress, int port = 18245)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new GeSRTPMessage();
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        var operateResult = ReadFromCoreServer(CommunicationPipe, new byte[56], hasResponseData: true, usePackAndUnpack: true);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(CommunicationPipe, new byte[56], hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度，返回原始的字节数组，支持 I,Q,M,T,SA,SB,SC,S,G 的位和字节读写，支持 AI,AQ,R 的字读写操作，地址示例：R1,M1<br />
    /// Batch read byte array information, need to specify the address and length, return the original byte array.
    /// Support I, Q, M, T, SA, SB, SC, S, G bit and byte read and write, support AI, AQ, R word read and write operations, address examples: R1, M1
    /// </summary>
    /// <remarks>
    /// 其中读取R，AI，AQ寄存器的原始字节时，传入的长度参数为字节长度。长度为10，返回10个字节数组信息，如果返回长度不满6个字节的，一律返回6个字节的数据
    /// </remarks>
    /// <param name="address">数据地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        var operateResult = GeHelper.BuildReadCommand(incrementCount.GetCurrentValue(), address, length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return GeHelper.ExtraResponseContent(operateResult2.Content);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, byte[] value)
    {
        var operateResult = GeHelper.BuildWriteCommand(incrementCount.GetCurrentValue(), address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return GeHelper.ExtraResponseContent(operateResult2.Content);
    }

    /// <summary>
    /// 根据指定的地址来读取一个字节的数据，按照字节为单位，例如 M1 字节，就是指 M1-M8 位组成的字节，M2 字节就是 M9-M16 组成的字节。不支持对 AI,AQ,R 寄存器的字节读取<br />
    /// A byte of data is read according to the specified address, and the unit is byte. For example, M1 byte refers to a byte composed of M1-M8 bits, 
    /// and M2 byte is a byte composed of M9-M16. Does not support byte reading of AI, AQ, R registers
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的 <see cref="T:System.Byte" /> 数据</returns>
    public OperateResult<byte> ReadByte(string address)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, 1, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(operateResult);
        }
        if (operateResult.Content.DataCode == 10 || operateResult.Content.DataCode == 12 || operateResult.Content.DataCode == 8)
        {
            return new OperateResult<byte>(StringResources.Language.GeSRTPNotSupportByteReadWrite);
        }
        return ByteTransformHelper.GetResultFromArray(Read(address, 1));
    }

    /// <summary>
    /// 向PLC中写入byte数据，返回是否写入成功<br />
    /// Write byte data to PLC and return whether the writing is successful
    /// </summary>
    /// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100 -&gt; Starting address, formatted as I100,mM100,Q100,DB20.100</param>
    /// <param name="value">byte数据 -&gt; Byte data</param>
    /// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
    public OperateResult Write(string address, byte value)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, 1, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(operateResult);
        }
        if (operateResult.Content.DataCode == 10 || operateResult.Content.DataCode == 12 || operateResult.Content.DataCode == 8)
        {
            return new OperateResult<byte>(StringResources.Language.GeSRTPNotSupportByteReadWrite);
        }
        return Write(address, new byte[1] { value });
    }

    /// <summary>
    /// 按照位为单位，批量从指定的地址里读取 bool 数组数据，不支持 AI，AQ，R 地址类型，地址比如从1开始，例如 I1,Q1,M1,T1,SA1,SB1,SC1,S1,G1<br />
    /// In units of bits, read bool array data from the specified address in batches. AI, AQ, 
    /// R address types are not supported. For example, the address starts from 1, such as I1, Q1, M1, T1, SA1, SB1, SC1, S1, G1
    /// </summary>
    /// <param name="address">PLC的地址信息，例如 M1, G1</param>
    /// <param name="length">读取的长度信息，按照位为单位</param>
    /// <returns>包含是否读取成功的bool数组</returns>
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, length, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var operateResult2 = GeHelper.BuildReadCommand(incrementCount.GetCurrentValue(), operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        var operateResult3 = ReadFromCoreServer(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult3);
        }
        var operateResult4 = GeHelper.ExtraResponseContent(operateResult3.Content);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult4);
        }
        return OperateResult.CreateSuccessResult(operateResult4.Content.ToBoolArray().SelectMiddle(operateResult.Content.AddressStart % 8, length));
    }

    /// <summary>
    /// 按照位为单位，批量写入 bool 数组到指定的地址里，不支持 AI，AQ，R 地址类型，地址比如从1开始，例如 I1,Q1,M1,T1,SA1,SB1,SC1,S1,G1<br />
    /// In units of bits, write bool arrays in batches to the specified addresses. AI, AQ, and R address types are not supported. For example, 
    /// the address starts from 1, such as I1, Q1, M1, T1, SA1, SB1, SC1, S1, G1
    /// </summary>
    /// <param name="address">PLC的地址信息，例如 M1, G1</param>
    /// <param name="value">bool 数组</param>
    /// <returns>是否写入成功的结果对象</returns>
    public override OperateResult Write(string address, bool[] value)
    {
        var operateResult = GeHelper.BuildWriteCommand(incrementCount.GetCurrentValue(), address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return GeHelper.ExtraResponseContent(operateResult2.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var build = GeHelper.BuildReadCommand(incrementCount.GetCurrentValue(), address, length, isBit: false);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        var build = GeHelper.BuildWriteCommand(incrementCount.GetCurrentValue(), address, value);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.ReadByte(System.String)" />
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
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.Write(System.String,System.Byte)" />
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
        return await WriteAsync(address, new byte[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var analysis = GeSRTPAddress.ParseFrom(address, length, isBit: true);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }
        var build = GeHelper.BuildReadCommand(incrementCount.GetCurrentValue(), analysis.Content);
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        var build = GeHelper.BuildWriteCommand(incrementCount.GetCurrentValue(), address, value);
        if (!build.IsSuccess)
        {
            return build;
        }
        var read = await ReadFromCoreServerAsync(build.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        return GeHelper.ExtraResponseContent(read.Content);
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取PLC当前的时间，这个时间可能是不包含时区的，需要自己转换成本地的时间。<br />
    /// <b>[Authorization]</b> Read the current time of the PLC, this time may not include the time zone, you need to convert the local time yourself.
    /// </summary>
    /// <returns>包含是否成功的时间信息</returns>
    public OperateResult<DateTime> ReadPLCTime()
    {
        var operateResult = GeHelper.BuildReadCoreCommand(incrementCount.GetCurrentValue(), 37, new byte[5] { 0, 0, 0, 2, 0 });
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(operateResult);
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(operateResult2);
        }
        var operateResult3 = GeHelper.ExtraResponseContent(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(operateResult3);
        }
        return GeHelper.ExtraDateTime(operateResult3.Content);
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取PLC当前的程序的名称<br />
    /// <b>[Authorization]</b> Read the name of the current program of the PLC
    /// </summary>
    /// <returns>包含是否成的程序名称信息</returns>
    public OperateResult<string> ReadProgramName()
    {
        var operateResult = GeHelper.BuildReadCoreCommand(incrementCount.GetCurrentValue(), 1, new byte[5] { 0, 0, 0, 2, 0 });
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult2);
        }
        var operateResult3 = GeHelper.ExtraResponseContent(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult3);
        }
        return GeHelper.ExtraProgramName(operateResult3.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.ReadPLCTime" />
    public async Task<OperateResult<DateTime>> ReadPLCTimeAsync()
    {
        var build = GeHelper.BuildReadCoreCommand(incrementCount.GetCurrentValue(), 37, new byte[5] { 0, 0, 0, 2, 0 });
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeSRTPNet.ReadProgramName" />
    public async Task<OperateResult<string>> ReadProgramNameAsync()
    {
        var build = GeHelper.BuildReadCoreCommand(incrementCount.GetCurrentValue(), 1, new byte[5] { 0, 0, 0, 2, 0 });
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content);
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
