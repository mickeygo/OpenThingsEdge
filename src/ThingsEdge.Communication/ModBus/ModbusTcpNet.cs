using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.ModBus;

/// <summary>
/// Modbus-Tcp协议的客户端通讯类，方便的和服务器进行数据交互，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见API文档说明。
/// </summary>
/// <remarks>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。
/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
/// </remarks>
/// <example>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，比如我们想要控制消息号在0-1000之间自增，不能超过一千，可以写如下的代码：
/// <note type="important">
/// 地址共可以携带4个信息，常见的使用表示方式"s=2;x=3;100"，对应的modbus报文是 02 03 00 64 00 01 的前四个字节，站号，功能码，起始地址，下面举例
/// </note>
/// 当读写int, uint, float, double, long, ulong类型的时候，支持动态指定数据格式，也就是 DataFormat 信息。
/// ReadInt32("format=BADC;100") 指示使用BADC的格式来解析byte数组，从而获得int数据，同时支持和站号信息叠加，例如：ReadInt32("format=BADC;s=2;100")
/// <list type="definition">
/// <item>
///     <term>读取线圈</term>
///     <description>ReadCoil("100")表示读取线圈100的值，ReadCoil("s=2;100")表示读取站号为2，线圈地址为100的值</description>
/// </item>
/// <item>
///     <term>读取离散输入</term>
///     <description>ReadDiscrete("100")表示读取离散输入100的值，ReadDiscrete("s=2;100")表示读取站号为2，离散地址为100的值</description>
/// </item>
/// <item>
///     <term>读取寄存器</term>
///     <description>ReadInt16("100")表示读取寄存器100的值，ReadInt16("s=2;100")表示读取站号为2，寄存器100的值</description>
/// </item>
/// <item>
///     <term>读取输入寄存器</term>
///     <description>ReadInt16("x=4;100")表示读取输入寄存器100的值，ReadInt16("s=2;x=4;100")表示读取站号为2，输入寄存器100的值</description>
/// </item>
/// <item>
///     <term>读取寄存器的位</term>
///     <description>ReadBool("100.0")表示读取寄存器100第0位的值，ReadBool("s=2;100.0")表示读取站号为2，寄存器100第0位的值，支持读连续的多个位</description>
/// </item>
/// <item>
///     <term>读取输入寄存器的位</term>
///     <description>ReadBool("x=4;100.0")表示读取输入寄存器100第0位的值，ReadBool("s=2;x=4;100.0")表示读取站号为2，输入寄存器100第0位的值，支持读连续的多个位</description>
/// </item>
/// </list>
/// 对于写入来说也是一致的
/// <list type="definition">
/// <item>
///     <term>写入线圈</term>
///     <description>WriteCoil("100",true)表示读取线圈100的值，WriteCoil("s=2;100",true)表示读取站号为2，线圈地址为100的值</description>
/// </item>
/// <item>
///     <term>写入寄存器</term>
///     <description>Write("100",(short)123)表示写寄存器100的值123，Write("s=2;100",(short)123)表示写入站号为2，寄存器100的值123</description>
/// </item>
/// </list>
/// 特殊说明部分：
/// 当碰到自定义的功能码时，比如读取功能码 07,写入功能码 08 的自定义寄存器时，地址可以这么写：
/// ReadInt16("s=2;x=7;w=8;100")表示读取这个自定义寄存器地址 100 的数据，读取使用的是 07 功能码，写入使用的是 08 功能码，也就是说 w=8 可以强制指定写入的功能码信息。
///  <list type="definition">
/// <item>
///     <term>01功能码</term>
///     <description>ReadBool("100")</description>
/// </item>
/// <item>
///     <term>02功能码</term>
///     <description>ReadBool("x=2;100")</description>
/// </item>
/// <item>
///     <term>03功能码</term>
///     <description>Read("100")</description>
/// </item>
/// <item>
///     <term>04功能码</term>
///     <description>Read("x=4;100")</description>
/// </item>
/// <item>
///     <term>05功能码</term>
///     <description>Write("100", True)</description>
/// </item>
/// <item>
///     <term>06功能码</term>
///     <description>Write("100", (short)100);Write("100", (ushort)100)</description>
/// </item>
/// <item>
///     <term>0F功能码</term>
///     <description>Write("100", new bool[]{True})   注意：这里和05功能码传递的参数类型不一样</description>
/// </item>
/// <item>
///     <term>10功能码</term>
///     <description>如果写一个short想用10功能码：Write("100", new short[]{100})</description>
/// </item>
/// <item>
///     <term>16功能码</term>
///     <description>Write("100.2", True) 当写入bool值的方法里，地址格式变为字地址时，就使用16功能码，通过掩码的方式来修改寄存器的某一位，
///     需要Modbus服务器支持，对于不支持该功能码的写入无效。</description>
/// </item>
/// </list>
/// <para>
/// 特别说明：调用写入 short 及 ushort 类型写入方法时，自动使用06功能码写入。需要需要使用 16 功能码写入 short 数值，可以参考下面的两种方式：
/// 1. modbus.Write("100", new short[]{ 1 });
/// 2. modbus.Write("w=16;100", (short)1);   (这个地址读short也能读取到正确的值)
/// </para>
/// </example>
public class ModbusTcpNet : DeviceTcpNet, IModbus, IReadWriteDevice, IReadWriteNet
{
    private Func<string, byte, OperateResult<string>> _addressMapping = (address, modbusCode) => OperateResult.CreateSuccessResult(address);

    /// <summary>
    /// 获取或设置起始的地址是否从0开始，默认为True。
    /// </summary>
    /// <remarks>
    /// <note type="warning">因为有些设备的起始地址是从1开始的，就要设置本属性为<c>False</c></note>
    /// </remarks>
    public bool AddressStartWithZero { get; set; } = true;

    /// <summary>
    /// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定，例如地址 "s=2;100", 更详细的例子可以参考DEMO界面上的地址示例。
    /// </summary>
    /// <remarks>
    /// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理。
    /// </remarks>
    public byte Station { get; set; } = 1;

    /// <inheritdoc cref="IByteTransform.DataFormat" />
    public DataFormat DataFormat
    {
        get
        {
            return ByteTransform.DataFormat;
        }
        set
        {
            ByteTransform.DataFormat = value;
        }
    }

    /// <summary>
    /// 字符串数据是否按照字来反转，默认为False。
    /// </summary>
    /// <remarks>
    /// 字符串按照2个字节的排列进行颠倒，根据实际情况进行设置
    /// </remarks>
    public bool IsStringReverse
    {
        get
        {
            return ByteTransform.IsStringReverseByteWord;
        }
        set
        {
            ByteTransform.IsStringReverseByteWord = value;
        }
    }

    /// <inheritdoc cref="IModbus.EnableWriteMaskCode" />
    public bool EnableWriteMaskCode { get; set; } = true;

    /// <inheritdoc cref="IMessage.ModbusTcpMessage.IsCheckMessageId" />
    public bool IsCheckMessageId { get; set; } = true;

    /// <inheritdoc cref="IModbus.BroadcastStation" />
    public int BroadcastStation { get; set; } = -1;

    /// <summary>
    /// 获取modbus协议自增的消息号，你可以自定义modbus的消息号的规则，详细参见<see cref="ModbusTcpNet" />说明，也可以查找<see cref="Common.SoftIncrementCount" />说明。
    /// </summary>
    public SoftIncrementCount MessageId { get; }

    /// <summary>
    /// 实例化一个Modbus-Tcp协议的客户端对象。
    /// </summary>
    public ModbusTcpNet()
    {
        MessageId = new SoftIncrementCount(65535L, 0L);
        WordLength = 1;
        Station = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    /// <summary>
    /// 指定服务器地址，端口号，客户端自己的站号来初始化。
    /// </summary>
    /// <param name="ipAddress">服务器的Ip地址</param>
    /// <param name="port">服务器的端口号</param>
    /// <param name="station">客户端自身的站号</param>
    public ModbusTcpNet(string ipAddress, int port = 502, byte station = 1)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
        Station = station;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new ModbusTcpMessage
        {
            IsCheckMessageId = IsCheckMessageId
        };
    }

    /// <summary>
    /// 将Modbus报文数据发送到当前的通道中，并从通道中接收Modbus的报文，通道将根据当前连接自动获取，本方法是线程安全的。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收到的Modbus报文信息</returns>
    /// <remarks>
    /// 需要注意的是，本方法的发送和接收都只需要输入Modbus核心报文，例如读取寄存器0的字数据 01 03 00 00 00 01，最前面的6个字节是自动添加的，收到的数据也是只有modbus核心报文，例如：01 03 02 00 00 , 所以在解析的时候需要注意。
    /// </remarks>
    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
    {
        if (BroadcastStation >= 0 && send[0] == BroadcastStation)
        {
            return ReadFromCoreServer(send, hasResponseData: false, usePackAndUnpack: true);
        }
        return await base.ReadFromCoreServerAsync(send).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => transform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => transform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => transform.TransSingle(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => transform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => transform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => transform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        if (_addressMapping != null)
        {
            return _addressMapping(address, modbusCode);
        }
        return OperateResult.CreateSuccessResult(address);
    }

    /// <inheritdoc cref="IModbus.RegisteredAddressMapping(Func{System.String,System.Byte,OperateResult{string}})" />
    public void RegisteredAddressMapping(Func<string, byte, OperateResult<string>> mapping)
    {
        _addressMapping = mapping;
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.PackCommandToTcp(command, (ushort)MessageId.GetCurrentValue());
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        if (BroadcastStation >= 0 && send[6] == BroadcastStation)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        if (response == null || response.Length < 6)
        {
            return new OperateResult<byte[]>(StringResources.Language.ReceiveDataLengthTooShort + " Content: " + response?.ToHexString(' '));
        }
        return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(response));
    }

    /// <inheritdoc cref="ModbusHelper.ReadWrite(HslCommunication.ModBus.IModbus,System.String,System.UInt16,System.String,System.Byte[])" />
    public OperateResult<byte[]> ReadWrite(string readAddress, ushort length, string writeAddress, byte[] value)
    {
        return ModbusHelper.ReadWrite(this, readAddress, length, writeAddress, value);
    }

    /// <summary>
    /// 批量的读取线圈，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x01<br />
    /// For batch reading coils, you need to specify the start address and read length. If the rich text address is not specified, the default function code is 0x01.
    /// </summary>
    /// <param name="address">起始地址，格式为"1234"</param>
    /// <returns>带有成功标志的bool数组对象</returns>
    public async Task<OperateResult<bool>> ReadCoilAsync(string address)
    {
        return await ReadBoolAsync(address).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量的读取线圈，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x01<br />
    /// For batch reading coils, you need to specify the start address and read length. If the rich text address is not specified, the default function code is 0x01.
    /// </summary>
    /// <param name="address">起始地址，格式为"1234"</param>
    /// <param name="length">读取长度</param>
    /// <returns>带有成功标志的bool数组对象</returns>
    public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
    {
        return await ReadBoolAsync(address, length).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取输入线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x02<br />
    /// To read the input coil, you need to specify the start address. If the rich text address is not specified, the default function code is 0x02.
    /// </summary>
    /// <param name="address">起始地址，格式为"1234"</param>
    /// <returns>带有成功标志的bool对象</returns>
    public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1).ConfigureAwait(false));
    }

    /// <summary>
    /// 读取输入线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x02<br />
    /// To read the input coil, you need to specify the start address. If the rich text address is not specified, the default function code is 0x02.
    /// </summary>
    /// <param name="address">起始地址，格式为"1234"</param>
    /// <param name="length">读取长度</param>
    /// <returns>带有成功标志的bool对象</returns>
    public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
    {
        return await ReadBoolHelperAsync(address, length, 2).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await ModbusHelper.ReadAsync(this, address, length).ConfigureAwait(false);
    }

    /// <inheritdoc cref=".ModbusHelper.ReadWrite(HslCommunication.ModBus.IModbus,System.String,System.UInt16,System.String,System.Byte[])" />
    public async Task<OperateResult<byte[]>> ReadWriteAsync(string readAddress, ushort length, string writeAddress, byte[] value)
    {
        return await ModbusHelper.ReadWriteAsync(this, readAddress, length, writeAddress, value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, short value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, ushort value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    /// <summary>
    /// 向设备写入掩码数据，使用0x16功能码，需要确认对方是否支持相关的操作，掩码数据的操作主要针对寄存器。<br />
    /// To write mask data to the server, using the 0x16 function code, you need to confirm whether the other party supports related operations. 
    /// The operation of mask data is mainly directed to the register.
    /// </summary>
    /// <param name="address">起始地址，起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
    /// <param name="andMask">等待与操作的掩码数据</param>
    /// <param name="orMask">等待或操作的掩码数据</param>
    /// <returns>是否写入成功</returns>
    public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
    {
        return await ModbusHelper.WriteMaskAsync(this, address, andMask, orMask).ConfigureAwait(false);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.Int16)" />
    public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
    {
        return await WriteAsync(address, value).ConfigureAwait(false);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.UInt16)" />
    public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
    {
        return await WriteAsync(address, value).ConfigureAwait(false);
    }

    private async Task<OperateResult<bool[]>> ReadBoolHelperAsync(string address, ushort length, byte function)
    {
        return await ModbusHelper.ReadBoolAsync(this, address, length, function).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await ReadBoolHelperAsync(address, length, 1).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await ModbusHelper.WriteAsync(this, address, values).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ModbusTcpNet[{IpAddress}:{Port}]";
    }
}
