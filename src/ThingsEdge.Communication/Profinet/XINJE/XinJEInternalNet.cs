using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.XINJE;

/// <summary>
/// 信捷内部的TCP信息，该协议是信捷基于modbus协议扩展而来，支持更多的地址类型，以及更广泛的地址范围。
/// </summary>
public class XinJEInternalNet : DeviceTcpNet
{
    private byte station = 1;

    private readonly SoftIncrementCount softIncrementCount;

    /// <summary>
    /// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定。
    /// </summary>
    /// <remarks>
    /// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理
    /// </remarks>
    public byte Station
    {
        get
        {
            return station;
        }
        set
        {
            station = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.IByteTransform.DataFormat" />
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
    /// 字符串数据是否按照字来反转，默认为False<br />
    /// Whether the string data is reversed according to words. The default is False.
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

    /// <summary>
    /// 获取协议自增的消息号，你可以自定义modbus的消息号的规则，详细参见<see cref="T:HslCommunication.Profinet.XINJE.XinJEInternalNet" />说明，也可以查找<see cref="T:HslCommunication.BasicFramework.SoftIncrementCount" />说明。<br />
    /// Get the message number incremented by the modbus protocol. You can customize the rules of the message number of the modbus. For details, please refer to the description of <see cref="T:HslCommunication.ModBus.ModbusTcpNet" />, or you can find the description of <see cref="T:HslCommunication.BasicFramework.SoftIncrementCount" />
    /// </summary>
    public SoftIncrementCount MessageId => softIncrementCount;

    /// <summary>
    /// 实例化一个XINJE-Tcp协议的客户端对象<br />
    /// Instantiate a client object of the Modbus-Tcp protocol
    /// </summary>
    public XinJEInternalNet()
    {
        softIncrementCount = new SoftIncrementCount(65535L, 0L);
        WordLength = 1;
        station = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    /// <summary>
    /// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
    /// Specify the server address, port number, and client's own station number to initialize
    /// </summary>
    /// <param name="ipAddress">服务器的Ip地址</param>
    /// <param name="port">服务器的端口号</param>
    /// <param name="station">客户端自身的站号</param>
    public XinJEInternalNet(string ipAddress, int port = 502, byte station = 1)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
        this.station = station;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new ModbusTcpMessage();
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.PackCommandToTcp(command, (ushort)softIncrementCount.GetCurrentValue());
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(response));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.XINJE.XinJEInternalNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = XinJEHelper.BuildReadCommand(Station, address, length, isBit: false);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        return await ReadFromCoreServerAsync(command.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.XINJE.XinJEInternalNet.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var command = XinJEHelper.BuildReadCommand(Station, address, length, isBit: true);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray().SelectBegin(length));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.XINJE.XinJEInternalNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        var command = XinJEHelper.BuildWriteWordCommand(Station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        return await ReadFromCoreServerAsync(command.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.XINJE.XinJEInternalNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        var command = XinJEHelper.BuildWriteBoolCommand(Station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        return await ReadFromCoreServerAsync(command.Content);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"XinJEInternalNet[{IpAddress}:{Port}]";
    }
}
