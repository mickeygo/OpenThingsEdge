using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯<br />
/// Mitsubishi PLC communication class is implemented using Qna compatible 3E frame protocol. 
/// The Ethernet module on the PLC side needs to be configured first. It must be binary communication.
/// </summary>
/// <remarks>
/// 支持读写的数据类型详细参考API文档，支持高级的数据读取，例如读取智能模块，缓冲存储器等等。如果使用的Fx5u的PLC，如果之前有写过用户认证，需要对设备信息全部初始化
/// </remarks>
/// <list type="number">
/// 目前组件测试通过的PLC型号列表，有些来自于网友的测试
/// <item>Q06UDV PLC  感谢hwdq0012</item>
/// <item>fx5u PLC  感谢山楂</item>
/// <item>Q02CPU PLC </item>
/// <item>L02CPU PLC </item>
/// </list>
/// 地址的输入的格式支持多种复杂的地址表示方式：
/// <list type="number">
/// <item>[商业授权] 扩展的数据地址: 表示为 ext=1;W100  访问扩展区域为1的W100的地址信息</item>
/// <item>[商业授权] 缓冲存储器地址: 表示为 mem=32  访问地址为32的本站缓冲存储器地址</item>
/// <item>[商业授权] 智能模块地址：表示为 module=3;4106  访问模块号3，偏移地址是4106的数据，偏移地址需要根据模块的详细信息来确认。</item>
/// <item>[商业授权] 基于标签的地址: 表示位 s=AAA  假如标签的名称为AAA，但是标签的读取是有条件的，详细参照<see cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadTags(System.String,System.UInt16)" /></item>
/// <item>普通的数据地址，参照下面的信息</item>
/// </list>
/// <example>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage" title="简单的短连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage2" title="简单的长连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample1" title="基本的读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample2" title="批量读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
/// </example>
public class MelsecMcNet : DeviceTcpNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.McType" />
    public virtual McType McType => McType.McBinary;

    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.NetworkNumber" />
    public byte NetworkNumber { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.PLCNumber" />
    public byte PLCNumber { get; set; } = byte.MaxValue;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.NetworkStationNumber" />
    public byte NetworkStationNumber { get; set; } = 0;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.EnableWriteBitToWordRegister" />
    public bool EnableWriteBitToWordRegister { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.TargetIOStation" />
    public ushort TargetIOStation { get; set; } = 1023;


    /// <summary>
    /// 实例化三菱的Qna兼容3E帧协议的通讯对象<br />
    /// Instantiate the communication object of Mitsubishi's Qna compatible 3E frame protocol
    /// </summary>
    public MelsecMcNet()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 指定ip地址和端口号来实例化一个默认的对象<br />
    /// Specify the IP address and port number to instantiate a default object
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public MelsecMcNet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecQnA3EBinaryMessage();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.McAnalysisAddress(System.String,System.UInt16,System.Boolean)" />
    public virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit)
    {
        return McAddressData.ParseMelsecFrom(address, length, isBit);
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return McBinaryHelper.PackMcCommand(this, command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        var operateResult = McBinaryHelper.CheckResponseContentHelper(response);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(11));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.ExtractActualData(System.Byte[],System.Boolean)" />
    public virtual byte[] ExtractActualData(byte[] response, bool isBit)
    {
        return McBinaryHelper.ExtractActualDataHelper(response, isBit);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.Read(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return McHelper.Read(this, address, length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return McHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.Read(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await McHelper.ReadAsync(this, address, length);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await McHelper.WriteAsync(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandom(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    [HslMqttApi("随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，需要自行解析数据")]
    public OperateResult<byte[]> ReadRandom(string[] address)
    {
        return McHelper.ReadRandom(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandom(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[],System.UInt16[])" />
    [HslMqttApi(ApiTopic = "ReadRandoms", Description = "随机读取PLC的数据信息，可以跨地址，跨类型组合，每个地址是任意的长度。收到结果后，需要自行解析数据，目前只支持字地址，比如D区，W区，R区，不支持X，Y，M，B，L等等")]
    public OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
    {
        return McHelper.ReadRandom(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandomInt16(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    public OperateResult<short[]> ReadRandomInt16(string[] address)
    {
        return McHelper.ReadRandomInt16(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandomUInt16(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    public OperateResult<ushort[]> ReadRandomUInt16(string[] address)
    {
        return McHelper.ReadRandomUInt16(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadRandom(System.String[])" />
    public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
    {
        return await McHelper.ReadRandomAsync(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadRandom(System.String[],System.UInt16[])" />
    public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address, ushort[] length)
    {
        return await McHelper.ReadRandomAsync(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadRandomInt16(System.String[])" />
    public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
    {
        return await McHelper.ReadRandomInt16Async(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadRandomUInt16(System.String[])" />
    public async Task<OperateResult<ushort[]>> ReadRandomUInt16Async(string[] address)
    {
        return await McHelper.ReadRandomUInt16Async(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String)" />
    [HslMqttApi("ReadBool", "")]
    public override OperateResult<bool> ReadBool(string address)
    {
        return base.ReadBool(address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16,System.Boolean)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return McHelper.ReadBool(this, address, length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return McHelper.Write(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String)" />
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return await base.ReadBoolAsync(address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16,System.Boolean)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await McHelper.ReadBoolAsync(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await McHelper.WriteAsync(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McBinaryHelper.ReadTags(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[],System.UInt16[])" />
    /// <param name="tag">数据标签</param>
    /// <param name="length">读取的数据长度</param>
    [HslMqttApi(ApiTopic = "ReadTag", Description = "读取PLC的标签信息，需要传入标签的名称，读取的字长度，标签举例：A; label[1]; bbb[10,10,10]")]
    public OperateResult<byte[]> ReadTags(string tag, ushort length)
    {
        return ReadTags(new string[1] { tag }, new ushort[1] { length });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadTags(System.String,System.UInt16)" />
    [HslMqttApi(ApiTopic = "ReadTags", Description = "批量读取PLC的标签信息，需要传入标签的名称，读取的字长度，标签举例：A; label[1]; bbb[10,10,10]")]
    public OperateResult<byte[]> ReadTags(string[] tags, ushort[] length)
    {
        return McBinaryHelper.ReadTags(this, tags, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadTags(System.String,System.UInt16)" />
    public async Task<OperateResult<byte[]>> ReadTagsAsync(string tag, ushort length)
    {
        return await ReadTagsAsync(new string[1] { tag }, new ushort[1] { length });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadTags(System.String,System.UInt16)" />
    public async Task<OperateResult<byte[]>> ReadTagsAsync(string[] tags, ushort[] length)
    {
        return await McBinaryHelper.ReadTagsAsync(this, tags, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadExtend(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.UInt16,System.String,System.UInt16)" />
    [HslMqttApi(ApiTopic = "ReadExtend", Description = "读取扩展的数据信息，需要在原有的地址，长度信息之外，输入扩展值信息")]
    public OperateResult<byte[]> ReadExtend(ushort extend, string address, ushort length)
    {
        return McHelper.ReadExtend(this, extend, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadExtend(System.UInt16,System.String,System.UInt16)" />
    public async Task<OperateResult<byte[]>> ReadExtendAsync(ushort extend, string address, ushort length)
    {
        return await McHelper.ReadExtendAsync(this, extend, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadMemory(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16)" />
    [HslMqttApi(ApiTopic = "ReadMemory", Description = "读取缓冲寄存器的数据信息，地址直接为偏移地址")]
    public OperateResult<byte[]> ReadMemory(string address, ushort length)
    {
        return McHelper.ReadMemory(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadMemory(System.String,System.UInt16)" />
    public async Task<OperateResult<byte[]>> ReadMemoryAsync(string address, ushort length)
    {
        return await McHelper.ReadMemoryAsync(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadSmartModule(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.UInt16,System.String,System.UInt16)" />
    [HslMqttApi(ApiTopic = "ReadSmartModule", Description = "读取智能模块的数据信息，需要指定模块地址，偏移地址，读取的字节长度")]
    public OperateResult<byte[]> ReadSmartModule(ushort module, string address, ushort length)
    {
        return McHelper.ReadSmartModule(this, module, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadSmartModule(System.UInt16,System.String,System.UInt16)" />
    public async Task<OperateResult<byte[]>> ReadSmartModuleAsync(ushort module, string address, ushort length)
    {
        return await McHelper.ReadSmartModuleAsync(this, module, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteRun(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    [HslMqttApi(ApiTopic = "RemoteRun", Description = "远程Run操作")]
    public OperateResult RemoteRun()
    {
        return McHelper.RemoteRun(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteStop(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    [HslMqttApi(ApiTopic = "RemoteStop", Description = "远程Stop操作")]
    public OperateResult RemoteStop()
    {
        return McHelper.RemoteStop(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteReset(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    [HslMqttApi(ApiTopic = "RemoteReset", Description = "LED 熄灭 出错代码初始化")]
    public OperateResult RemoteReset()
    {
        return McHelper.RemoteReset(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadPlcType(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    [HslMqttApi(ApiTopic = "ReadPlcType", Description = "读取PLC的型号信息，例如 Q02HCPU")]
    public OperateResult<string> ReadPlcType()
    {
        return McHelper.ReadPlcType(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ErrorStateReset(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    [HslMqttApi(ApiTopic = "ErrorStateReset", Description = "LED 熄灭 出错代码初始化")]
    public OperateResult ErrorStateReset()
    {
        return McHelper.ErrorStateReset(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.RemoteRun" />
    public async Task<OperateResult> RemoteRunAsync()
    {
        return await McHelper.RemoteRunAsync(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.RemoteStop" />
    public async Task<OperateResult> RemoteStopAsync()
    {
        return await McHelper.RemoteStopAsync(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.RemoteReset" />
    public async Task<OperateResult> RemoteResetAsync()
    {
        return await McHelper.RemoteResetAsync(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ReadPlcType" />
    public async Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return await McHelper.ReadPlcTypeAsync(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.ErrorStateReset" />
    public async Task<OperateResult> ErrorStateResetAsync()
    {
        return await McHelper.ErrorStateResetAsync(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecMcNet[{IpAddress}:{Port}]";
    }
}
