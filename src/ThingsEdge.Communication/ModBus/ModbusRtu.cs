using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.ModBus;

/// <summary>
/// Modbus-Rtu通讯协议的类库，多项式码0xA001，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式。
/// </summary>
/// <remarks>
/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式。
/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
/// </remarks>
public class ModbusRtu : DeviceSerialPort, IModbus, IReadWriteDevice, IReadWriteNet
{
    private Func<string, byte, OperateResult<string>> _addressMapping = (address, modbusCode) => OperateResult.CreateSuccessResult(address);

    /// <inheritdoc cref="ModbusTcpNet.AddressStartWithZero" />
    public bool AddressStartWithZero { get; set; } = true;

    /// <inheritdoc cref="ModbusTcpNet.Station" />
    public byte Station { get; set; } = 1;

    /// <inheritdoc cref="ModbusTcpNet.DataFormat" />
    public DataFormat DataFormat
    {
        get => ByteTransform.DataFormat;
        set => ByteTransform.DataFormat = value;
    }

    /// <inheritdoc cref="ModbusTcpNet.IsStringReverse" />
    public bool IsStringReverse
    {
        get => ByteTransform.IsStringReverseByteWord;
        set => ByteTransform.IsStringReverseByteWord = value;
    }

    /// <inheritdoc cref="IModbus.EnableWriteMaskCode" />
    public bool EnableWriteMaskCode { get; set; } = true;

    public int BroadcastStation { get; set; } = -1;

    /// <summary>
    /// 获取或设置当前是否启用站号检查的功能，默认启用，读写数据时将进行站号确认操作，如果需要忽略站号，则设置为 false 即可。
    /// </summary>
    public bool StationCheckMacth { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用CRC16校验码的检查功能，默认启用，如果需要忽略检查CRC16，则设置为 false 即可。
    /// </summary>
    public bool Crc16CheckEnable { get; set; } = true;

    /// <summary>
    /// 实例化一个Modbus-Rtu协议的客户端对象。
    /// </summary>
    public ModbusRtu()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
        ReceiveEmptyDataCount = 8;
    }

    /// <summary>
    /// 指定Modbus从站的站号来初始化。
    /// </summary>
    /// <param name="station">Modbus从站的站号</param>
    public ModbusRtu(byte station = 1)
        : this()
    {
        Station = station;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new ModbusRtuMessage(StationCheckMacth);
    }

    public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        if (_addressMapping != null)
        {
            return _addressMapping(address, modbusCode);
        }
        return OperateResult.CreateSuccessResult(address);
    }

    public void RegisteredAddressMapping(Func<string, byte, OperateResult<string>> mapping)
    {
        _addressMapping = mapping;
    }

    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.PackCommandToRtu(command);
    }

    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return ModbusHelper.ExtraRtuResponseContent(send, response, Crc16CheckEnable, BroadcastStation);
    }

    /// <summary>
    /// 将Modbus报文数据发送到当前的通道中，并从通道中接收Modbus的报文，通道将根据当前连接自动获取，本方法是线程安全的。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收到的Modbus报文信息</returns>
    /// <remarks>
    /// 需要注意的是，本方法的发送和接收都只需要输入Modbus核心报文，例如读取寄存器0的字数据 01 03 00 00 00 01，最后面两个字节的CRC是自动添加的，
    /// 收到的数据也是只有modbus核心报文，例如：01 03 02 00 00 , 已经成功校验CRC校验并移除了，所以在解析的时候需要注意。
    /// </remarks>
    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
    {
        if (BroadcastStation >= 0 && send[0] == BroadcastStation)
        {
            return await ReadFromCoreServerAsync(send, hasResponseData: false, usePackAndUnpack: true).ConfigureAwait(false);
        }
        return await base.ReadFromCoreServerAsync(send).ConfigureAwait(false);
    }

    public async Task<OperateResult<bool>> ReadCoilAsync(string address)
    {
        return await ReadBoolAsync(address).ConfigureAwait(false);
    }

    public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
    {
        return await ReadBoolAsync(address, length).ConfigureAwait(false);
    }

    public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1).ConfigureAwait(false));
    }

    public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
    {
        return await ModbusHelper.ReadBoolAsync(this, address, length, 2).ConfigureAwait(false);
    }

    public async Task<OperateResult<byte[]>> ReadWriteAsync(string readAddress, ushort length, string writeAddress, byte[] value)
    {
        return await ModbusHelper.ReadWriteAsync(this, readAddress, length, writeAddress, value).ConfigureAwait(false);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await ModbusHelper.ReadAsync(this, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await ModbusHelper.ReadBoolAsync(this, address, length, 1).ConfigureAwait(false);
    }

    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 2)).ConfigureAwait(false),
            (m) => transform.TransInt32(m, 0, length));
    }

    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 2)).ConfigureAwait(false),
            (m) => transform.TransUInt32(m, 0, length));
    }

    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 2)).ConfigureAwait(false),
            (m) => transform.TransSingle(m, 0, length));
    }

    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 4)).ConfigureAwait(false),
            (m) => transform.TransInt64(m, 0, length));
    }

    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 4)).ConfigureAwait(false),
            (m) => transform.TransUInt64(m, 0, length));
    }

    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(
            await ReadAsync(address, (ushort)(length * WordLength * 4)).ConfigureAwait(false),
            (m) => transform.TransDouble(m, 0, length));
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await ModbusHelper.WriteAsync(this, address, data).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await ModbusHelper.WriteAsync(this, address, values).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, short value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ushort value)
    {
        return await ModbusHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteAsync(address, CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values)).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
    {
        return await ModbusHelper.WriteMaskAsync(this, address, andMask, orMask).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
    {
        return await WriteAsync(address, value).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
    {
        return await WriteAsync(address, value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ModbusRtu[{PortName}:{BaudRate}]";
    }
}