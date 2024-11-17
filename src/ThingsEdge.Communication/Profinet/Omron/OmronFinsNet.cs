using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC通讯类，采用Fins-Tcp通信协议实现，支持的地址信息参见api文档信息。本协议下PLC默认的端口号为 9600，也可以手动更改，重启PLC更改生效。<br />
/// Omron PLC communication class is implemented using Fins-Tcp communication protocol. For the supported address information, please refer to the api document information.
/// The default port number of the PLC under this protocol is 9600, and it can also be changed manually. Restart the PLC to make the changes take effect.
/// </summary>
/// <remarks>
/// <note type="important">PLC的IP地址的要求，最后一个整数的范围应该小于250，否则会发生连接不上的情况。</note>
/// <br />
/// <note type="warning">如果在测试的时候报错误码64，经网友 上海-Lex 指点，是因为PLC中产生了报警，如伺服报警，模块错误等产生的，但是数据还是能正常读到的，屏蔽64报警或清除plc错误可解决</note>
/// <br />
/// <note type="warning">如果碰到NX系列连接失败，或是无法读取的，需要使用网口2，配置ip地址，网线连接网口2，配置FINSTCP，把UDP的端口改成9601的，这样就可以读写了。</note><br />
/// 需要特别注意<see cref="P:HslCommunication.Profinet.Omron.OmronFinsNet.ReadSplits" />属性，在超长数据读取时，规定了切割读取的长度，在不是CP1H及扩展模块的时候，可以设置为999，提高一倍的通信速度。
/// </remarks>
/// <example>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="Usage" title="简单的短连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="Usage2" title="简单的长连接使用" />
/// 下面演示下各种类型的读写操作
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample1" title="各种类型读取的示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample1" title="各种类型写入的示例" />
/// 如果想要一次性读取不同类型的数据的话，可以读取byte[]，然后自行解析
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample2" title="自定义解析读取" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample2" title="自定义解析写入" />
/// 读写bool的示例代码
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadBool" title="读取bool示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteBool" title="写入bool示例" />
/// 以下演示随机字读取的例子，在某些情况下可以更加的高性能
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadMulti" title="随机读取示例" />
/// </example>
public class OmronFinsNet : DeviceTcpNet, IOmronFins, IReadWriteDevice, IReadWriteNet
{
    private readonly byte[] handSingle = new byte[20]
    {
        70, 73, 78, 83, 0, 0, 0, 12, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    private readonly SoftIncrementCount incrementSID = new SoftIncrementCount(255L, 0L);

    /// <summary>
    /// 信息控制字段，默认0x80<br />
    /// Information control field, default 0x80
    /// </summary>
    public byte ICF { get; set; } = 128;


    /// <summary>
    /// 系统使用的内部信息<br />
    /// Internal information used by the system
    /// </summary>
    public byte RSV { get; private set; } = 0;


    /// <summary>
    /// 网络层信息，默认0x02，如果有八层消息，就设置为0x07<br />
    /// Network layer information, default is 0x02, if there are eight layers of messages, set to 0x07
    /// </summary>
    public byte GCT { get; set; } = 2;


    /// <summary>
    /// PLC的网络号地址，默认0x00<br />
    /// PLC network number address, default 0x00
    /// </summary>
    /// <remarks>
    /// 00: Local network<br />
    /// 01-7F: Remote network address (decimal: 1 to 127)
    /// </remarks>
    public byte DNA { get; set; } = 0;


    /// <summary>
    /// PLC的节点地址，默认为0，在和PLC连接的过程中，自动从PLC获取到DA1的值。<br />
    /// The node address of the PLC is 0 by default. During the process of connecting with the PLC, the value of DA1 is automatically obtained from the PLC.
    /// </summary>
    [HslMqttApi(HttpMethod = "GET", Description = "The node address of the PLC is 0 by default. During the process of connecting with the PLC, the value of DA1 is automatically obtained from the PLC.")]
    public byte DA1 { get; set; } = 0;


    /// <summary>
    /// PLC的单元号地址，通常都为0<br />
    /// PLC unit number address, usually 0
    /// </summary>
    /// <remarks>
    /// 00: CPU Unit<br />
    /// FE: Controller Link Unit or Ethernet Unit connected to network<br />
    /// 10 TO 1F: CPU Bus Unit<br />
    /// E1: Inner Board
    /// </remarks>
    public byte DA2 { get; set; } = 0;


    /// <summary>
    /// 上位机的网络号地址<br />
    /// Network number and address of the computer
    /// </summary>
    /// <remarks>
    /// 00: Local network<br />
    /// 01-7F: Remote network (1 to 127 decimal)
    /// </remarks>
    public byte SNA { get; set; } = 0;


    /// <summary>
    /// 上位机的节点地址，默认是0x01，当连接PLC之后，将由PLC来设定当前的值。<br />
    /// The node address of the host computer is 0x01 by default. After connecting to the PLC, the PLC will set the current value.
    /// </summary>
    /// <remarks>
    /// <note type="important">v9.6.5版本及之前的版本都需要手动设置，如果是多连接，相同的节点是连接不上PLC的。</note>
    /// </remarks>
    [HslMqttApi(HttpMethod = "GET", Description = "The node address of the host computer is 0x01 by default. After connecting to the PLC, the PLC will set the current value.")]
    public byte SA1 { get; set; } = 1;


    /// <summary>
    /// 上位机的单元号地址<br />
    /// Unit number and address of the computer
    /// </summary>
    /// <remarks>
    /// 00: CPU Unit<br />
    /// 10-1F: CPU Bus Unit
    /// </remarks>
    public byte SA2 { get; set; }

    /// <summary>
    /// 服务的标识号，由客户端生成自增的顺序值，用来标识和校验通信报文的ID<br />
    /// Service ID. Used to identify the process generating the transmission. Set the SID to any number between 00 and FF
    /// </summary>
    public byte SID { get; set; } = 0;


    /// <summary>
    /// 进行字读取的时候对于超长的情况按照本属性进行切割，默认500，如果不是CP1H及扩展模块的，可以设置为999，可以提高一倍的通信速度。<br />
    /// When reading words, it is cut according to this attribute for the case of overlength. The default is 500. 
    /// If it is not for CP1H and expansion modules, it can be set to 999, which can double the communication speed.
    /// </summary>
    public int ReadSplits { get; set; } = 500;


    /// <summary>
    /// 当接收PLC返回的数据的时候，获取或设置是否需要接收数据直到空为止，在一些及其特殊的场景里，可以设置为 true 防止数据错误的情况。<br />
    /// When receiving the data returned by the PLC, it is necessary to obtain or set whether to receive data until empty, 
    /// and in some special scenarios, it can be set to true to prevent data errors.
    /// </summary>
    public bool ReceiveUntilEmpty { get; set; } = false;


    /// <inheritdoc cref="P:HslCommunication.Profinet.Omron.Helper.IOmronFins.PlcType" />
    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;


    /// <summary>
    /// 实例化一个欧姆龙PLC Fins帧协议的通讯对象<br />
    /// Instantiate a communication object of Omron PLC Fins frame protocol
    /// </summary>
    public OmronFinsNet()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        ByteTransform.IsStringReverseByteWord = true;
    }

    /// <summary>
    /// 指定ip地址和端口号来实例化一个欧姆龙PLC Fins帧协议的通讯对象<br />
    /// Specify the IP address and port number to instantiate a communication object of the Omron PLC Fins frame protocol
    /// </summary>
    /// <param name="ipAddress">PLCd的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public OmronFinsNet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FinsMessage();
    }

    /// <summary>
    /// 将普通的指令打包成完整的指令
    /// </summary>
    /// <param name="cmd">FINS的核心指令</param>
    /// <returns>完整的可用于发送PLC的命令</returns>
    private byte[] PackCommand(byte[] cmd)
    {
        var array = new byte[26 + cmd.Length];
        Array.Copy(handSingle, 0, array, 0, 4);
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
        array[25] = (byte)incrementSID.GetCurrentValue();
        cmd.CopyTo(array, 26);
        SID = array[25];
        return array;
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        var operateResult = ReadFromCoreServer(CommunicationPipe, handSingle, hasResponseData: true, usePackAndUnpack: false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var num = BitConverter.ToInt32(new byte[4]
        {
            operateResult.Content[15],
            operateResult.Content[14],
            operateResult.Content[13],
            operateResult.Content[12]
        }, 0);
        if (num != 0)
        {
            return new OperateResult(num, OmronFinsNetHelper.GetStatusDescription(num));
        }
        if (operateResult.Content.Length >= 20)
        {
            SA1 = operateResult.Content[19];
        }
        if (operateResult.Content.Length >= 24)
        {
            DA1 = operateResult.Content[23];
        }
        incrementSID.ResetStartValue(0L);
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(CommunicationPipe, handSingle, hasResponseData: true, usePackAndUnpack: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var status = BitConverter.ToInt32(new byte[4]
        {
            read.Content[15],
            read.Content[14],
            read.Content[13],
            read.Content[12]
        }, 0);
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
        incrementSID.ResetStartValue(0L);
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return PackCommand(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronFinsNetHelper.ResponseValidAnalysis(response);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Read(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String,System.UInt16,System.Int32)" />
    /// <example>
    /// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102,D103存储了产量计数，读取如下：
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample2" title="Read示例" />
    /// 以下是读取不同类型数据的示例
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample1" title="Read示例" />
    /// </example>
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return OmronFinsNetHelper.Read(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Write(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String,System.Byte[])" />
    /// <example>
    /// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102,D103存储了产量计数，读取如下：
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample2" title="Write示例" />
    /// 以下是写入不同类型数据的示例
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample1" title="Write示例" />
    /// </example>
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return OmronFinsNetHelper.Write(this, address, value);
    }

    /// <inheritdoc />
    [HslMqttApi("ReadString", "")]
    public override OperateResult<string> ReadString(string address, ushort length)
    {
        return base.ReadString(address, length, Encoding.UTF8);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteString", "")]
    public override OperateResult Write(string address, string value)
    {
        return base.Write(address, value, Encoding.UTF8);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Read(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String[])" />
    public OperateResult<byte[]> Read(string[] address)
    {
        return OmronFinsNetHelper.Read(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await OmronFinsNetHelper.ReadAsync(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await OmronFinsNetHelper.WriteAsync(this, address, value);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await base.ReadStringAsync(address, length, Encoding.UTF8);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await base.WriteAsync(address, value, Encoding.UTF8);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Read(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String[])" />
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address)
    {
        return await OmronFinsNetHelper.ReadAsync(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadBool(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String,System.UInt16,System.Int32)" />
    /// <example>
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadBool" title="ReadBool示例" />
    /// </example>
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return OmronFinsNetHelper.ReadBool(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Write(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String,System.Boolean[])" />
    /// <example>
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteBool" title="WriteBool示例" />
    /// </example>
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return OmronFinsNetHelper.Write(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await OmronFinsNetHelper.ReadBoolAsync(this, address, length, ReadSplits);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await OmronFinsNetHelper.WriteAsync(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Run(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "Run", Description = "将CPU单元的操作模式更改为RUN，从而使PLC能够执行其程序。")]
    public OperateResult Run()
    {
        return OmronFinsNetHelper.Run(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Stop(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "Stop", Description = "将CPU单元的操作模式更改为PROGRAM，停止程序执行。")]
    public OperateResult Stop()
    {
        return OmronFinsNetHelper.Stop(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitData(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuUnitData", Description = "读取CPU的一些数据信息，主要包含型号，版本，一些数据块的大小。")]
    public OperateResult<OmronCpuUnitData> ReadCpuUnitData()
    {
        return OmronFinsNetHelper.ReadCpuUnitData(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitStatus(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuUnitStatus", Description = "读取CPU单元的一些操作状态数据，主要包含运行状态，工作模式，错误信息等。")]
    public OperateResult<OmronCpuUnitStatus> ReadCpuUnitStatus()
    {
        return OmronFinsNetHelper.ReadCpuUnitStatus(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuTime(HslCommunication.Core.IReadWriteDevice)" />
    [HslMqttApi(ApiTopic = "ReadCpuTime", Description = "读取CPU单元的时间信息。")]
    public OperateResult<DateTime> ReadCpuTime()
    {
        return OmronFinsNetHelper.ReadCpuTime(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Run(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult> RunAsync()
    {
        return await OmronFinsNetHelper.RunAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Stop(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult> StopAsync()
    {
        return await OmronFinsNetHelper.StopAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitData(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync()
    {
        return await OmronFinsNetHelper.ReadCpuUnitDataAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuUnitStatus(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync()
    {
        return await OmronFinsNetHelper.ReadCpuUnitStatusAsync(this).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.ReadCpuTime(HslCommunication.Core.IReadWriteDevice)" />
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
