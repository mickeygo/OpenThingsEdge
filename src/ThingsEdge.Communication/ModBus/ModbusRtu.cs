using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.HslCommunication;

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
        get
        {
            return ByteTransform.DataFormat;
        }
        set
        {
            ByteTransform.DataFormat = value;
        }
    }

    /// <inheritdoc cref="ModbusTcpNet.IsStringReverse" />
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


    /// <inheritdoc cref="IModbus.BroadcastStation" />
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

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new ModbusRtuMessage(StationCheckMacth);
    }

    /// <inheritdoc cref="IModbus.TranslateToModbusAddress(System.String,System.Byte)" />
    public virtual OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        if (_addressMapping != null)
        {
            return _addressMapping(address, modbusCode);
        }
        return OperateResult.CreateSuccessResult(address);
    }

    /// <inheritdoc cref="IModbus.RegisteredAddressMapping(Func{System.String,System.Byte,HslCommunication.OperateResult{System.String}})" />
    public void RegisteredAddressMapping(Func<string, byte, OperateResult<string>> mapping)
    {
        _addressMapping = mapping;
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return ModbusInfo.PackCommandToRtu(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return ModbusHelper.ExtraRtuResponseContent(send, response, Crc16CheckEnable, BroadcastStation);
    }

    /// <summary>
    /// 将Modbus报文数据发送到当前的通道中，并从通道中接收Modbus的报文，通道将根据当前连接自动获取，本方法是线程安全的。<br />
    /// Send Modbus message data to the current channel, and receive Modbus messages from the channel. The channel will automatically obtain it according to the current connection. This method is thread-safe.
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收到的Modbus报文信息</returns>
    /// <remarks>
    /// 需要注意的是，本方法的发送和接收都只需要输入Modbus核心报文，例如读取寄存器0的字数据 01 03 00 00 00 01，最后面两个字节的CRC是自动添加的，收到的数据也是只有modbus核心报文，例如：01 03 02 00 00 , 已经成功校验CRC校验并移除了，所以在解析的时候需要注意。<br />
    /// It should be noted that the sending and receiving of this method only need to input Modbus core messages, for example, read the word data 01 03 00 00 00 01 of register 0, the last two bytes of CRC are automatically added, 
    /// and received The data is also only modbus core messages, for example: 01 03 02 00 00, CRC has been successfully checked and removed, so you need to pay attention when parsing.
    /// </remarks>
    public override OperateResult<byte[]> ReadFromCoreServer(byte[] send)
    {
        if (BroadcastStation >= 0 && send[0] == BroadcastStation)
        {
            return ReadFromCoreServer(send, hasResponseData: false, usePackAndUnpack: true);
        }
        return base.ReadFromCoreServer(send);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.ReadFromCoreServer(System.Byte[])" />
    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send)
    {
        if (BroadcastStation >= 0 && send[0] == BroadcastStation)
        {
            return ReadFromCoreServer(send, hasResponseData: false, usePackAndUnpack: true);
        }
        return await base.ReadFromCoreServerAsync(send);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.ReadCoil(System.String)" />
    public OperateResult<bool> ReadCoil(string address)
    {
        return ReadBool(address);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.ReadCoil(System.String,System.UInt16)" />
    public OperateResult<bool[]> ReadCoil(string address, ushort length)
    {
        return ReadBool(address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.ReadDiscrete(System.String)" />
    public OperateResult<bool> ReadDiscrete(string address)
    {
        return ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.ReadDiscrete(System.String,System.UInt16)" />
    public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
    {
        return ModbusHelper.ReadBoolHelper(this, address, length, 2);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return ModbusHelper.Read(this, address, length);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusHelper.ReadWrite(HslCommunication.ModBus.IModbus,System.String,System.UInt16,System.String,System.Byte[])" />
    [HslMqttApi("ReadWrite", "Use 0x17 function code to write and read data at the same time, and use a message to implement it")]
    public OperateResult<byte[]> ReadWrite(string readAddress, ushort length, string writeAddress, byte[] value)
    {
        return ModbusHelper.ReadWrite(this, readAddress, length, writeAddress, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return ModbusHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.Int16)" />
    [HslMqttApi("WriteInt16", "")]
    public override OperateResult Write(string address, short value)
    {
        return ModbusHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.UInt16)" />
    [HslMqttApi("WriteUInt16", "")]
    public override OperateResult Write(string address, ushort value)
    {
        return ModbusHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.WriteMask(System.String,System.UInt16,System.UInt16)" />
    [HslMqttApi("WriteMask", "")]
    public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
    {
        return ModbusHelper.WriteMask(this, address, andMask, orMask);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.Write(System.String,System.Int16)" />
    public OperateResult WriteOneRegister(string address, short value)
    {
        return Write(address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.Write(System.String,System.UInt16)" />
    public OperateResult WriteOneRegister(string address, ushort value)
    {
        return Write(address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.Write(System.String,System.Int16)" />/param&gt;
    public override async Task<OperateResult> WriteAsync(string address, short value)
    {
        return await Task.Run(() => Write(address, value));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.Write(System.String,System.UInt16)" />/param&gt;
    public override async Task<OperateResult> WriteAsync(string address, ushort value)
    {
        return await Task.Run(() => Write(address, value));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.ReadCoil(System.String)" />
    public async Task<OperateResult<bool>> ReadCoilAsync(string address)
    {
        return await Task.Run(() => ReadCoil(address));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.ReadCoil(System.String,System.UInt16)" />
    public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
    {
        return await Task.Run(() => ReadCoil(address, length));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.ReadDiscrete(System.String)" />
    public async Task<OperateResult<bool>> ReadDiscreteAsync(string address)
    {
        return await Task.Run(() => ReadDiscrete(address));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.ReadDiscrete(System.String,System.UInt16)" />
    public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
    {
        return await Task.Run(() => ReadDiscrete(address, length));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.WriteOneRegister(System.String,System.Int16)" />
    public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
    {
        return await Task.Run(() => WriteOneRegister(address, value));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.WriteOneRegister(System.String,System.UInt16)" />
    public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
    {
        return await Task.Run(() => WriteOneRegister(address, value));
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.WriteMask(System.String,System.UInt16,System.UInt16)" />
    public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
    {
        return await Task.Run(() => WriteMask(address, andMask, orMask));
    }

    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return ModbusHelper.ReadBoolHelper(this, address, length, 1);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] values)
    {
        return ModbusHelper.Write(this, address, values);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusTcpNet.Write(System.String,System.Boolean)" />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        return ModbusHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.ModBus.ModbusRtu.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await Task.Run(() => Write(address, value));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32(System.String,System.UInt16)" />
    [HslMqttApi("ReadInt32Array", "")]
    public override OperateResult<int[]> ReadInt32(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (m) => transform.TransInt32(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32(System.String,System.UInt16)" />
    [HslMqttApi("ReadUInt32Array", "")]
    public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (m) => transform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloat(System.String,System.UInt16)" />
    [HslMqttApi("ReadFloatArray", "")]
    public override OperateResult<float[]> ReadFloat(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (m) => transform.TransSingle(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64(System.String,System.UInt16)" />
    [HslMqttApi("ReadInt64Array", "")]
    public override OperateResult<long[]> ReadInt64(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (m) => transform.TransInt64(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64(System.String,System.UInt16)" />
    [HslMqttApi("ReadUInt64Array", "")]
    public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (m) => transform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDouble(System.String,System.UInt16)" />
    [HslMqttApi("ReadDoubleArray", "")]
    public override OperateResult<double[]> ReadDouble(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (m) => transform.TransDouble(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int32[])" />
    [HslMqttApi("WriteInt32Array", "")]
    public override OperateResult Write(string address, int[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt32[])" />
    [HslMqttApi("WriteUInt32Array", "")]
    public override OperateResult Write(string address, uint[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Single[])" />
    [HslMqttApi("WriteFloatArray", "")]
    public override OperateResult Write(string address, float[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int64[])" />
    [HslMqttApi("WriteInt64Array", "")]
    public override OperateResult Write(string address, long[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt64[])" />
    [HslMqttApi("WriteUInt64Array", "")]
    public override OperateResult Write(string address, ulong[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Double[])" />
    [HslMqttApi("WriteDoubleArray", "")]
    public override OperateResult Write(string address, double[] values)
    {
        var byteTransform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return Write(address, byteTransform.TransByte(values));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32Async(System.String,System.UInt16)" />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (m) => transform.TransInt32(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32Async(System.String,System.UInt16)" />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (m) => transform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloatAsync(System.String,System.UInt16)" />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (m) => transform.TransSingle(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64Async(System.String,System.UInt16)" />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (m) => transform.TransInt64(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64Async(System.String,System.UInt16)" />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (m) => transform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDoubleAsync(System.String,System.UInt16)" />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        var transform = CommHelper.ExtractTransformParameter(ref address, ByteTransform);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (m) => transform.TransDouble(m, 0, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int32[])" />
    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt32[])" />
    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Single[])" />
    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int64[])" />
    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt64[])" />
    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Double[])" />
    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteAsync(value: CommHelper.ExtractTransformParameter(ref address, ByteTransform).TransByte(values), address: address);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ModbusRtu[{PortName}:{BaudRate}]";
    }
}
