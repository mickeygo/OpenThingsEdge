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
    private readonly IncrementCounter _counter = new(65535L, 0L);

    /// <summary>
    /// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定。
    /// </summary>
    /// <remarks>
    /// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理
    /// </remarks>
    public byte Station { get; set; } = 1;

    public DataFormat DataFormat
    {
        get => ByteTransform.DataFormat;
        set => ByteTransform.DataFormat = value;
    }

    /// <summary>
    /// 字符串数据是否按照字来反转，默认为False。
    /// </summary>
    /// <remarks>
    /// 字符串按照2个字节的排列进行颠倒，根据实际情况进行设置
    /// </remarks>
    public bool IsStringReverse
    {
        get => ByteTransform.IsStringReverseByteWord;
        set => ByteTransform.IsStringReverseByteWord = value;
    }

    /// <summary>
    /// 获取协议自增的消息号，可以自定义modbus的消息号的规则。
    /// </summary>
    public IncrementCounter MessageId => _counter;

    /// <summary>
    /// 指定服务器地址，端口号，客户端自己的站号来初始化。
    /// </summary>
    /// <param name="ipAddress">服务器的Ip地址</param>
    /// <param name="port">服务器的端口号</param>
    /// <param name="station">客户端自身的站号</param>
    /// <param name="options">选项</param>
    public XinJEInternalNet(string ipAddress, int port = 502, byte station = 1, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        Station = station;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new ModbusTcpMessage();
    }

    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.PackCommandToTcp(command, (ushort)_counter.OnNext());
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(response));
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = XinJEHelper.BuildReadCommand(Station, address, length, isBit: false);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        return await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var command = XinJEHelper.BuildReadCommand(Station, address, length, isBit: true);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray().SelectBegin(length));
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        var command = XinJEHelper.BuildWriteWordCommand(Station, address, data);
        if (!command.IsSuccess)
        {
            return command;
        }
        return await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        var command = XinJEHelper.BuildWriteBoolCommand(Station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        return await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"XinJEInternalNet[{Host}:{Port}]";
    }
}
