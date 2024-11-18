using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AllenBradley品牌的PLC，针对SLC系列的通信的实现，测试PLC为1747。<br />
/// AllenBradley brand PLC, for the realization of SLC series communication, the test PLC is 1747.
/// </summary>
/// <remarks>
/// 地址支持的举例：A9:0, N9:0, B9:0, F9:0, S:0, ST1:0, C9:0, I:0/10, O:0/1, R9:0, T9:0, L9:0
/// </remarks>
/// <example>
/// 地址格式如下：
/// <list type="table">
///   <listheader>
///     <term>地址代号</term>
///     <term>字操作</term>
///     <term>位操作</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>A</term>
///     <term>A9:0</term>
///     <term>A9:0/1 或 A9:0.1</term>
///     <term>ASCII</term>
///   </item>
///   <item>
///     <term>B</term>
///     <term>B9:0</term>
///     <term>B9:0/1 或 B9:0.1</term>
///     <term>Bit</term>
///   </item>
///   <item>
///     <term>N</term>
///     <term>N9:0</term>
///     <term>N9:0/1 或 N9:0.1</term>
///     <term>Integer</term>
///   </item>
///   <item>
///     <term>F</term>
///     <term>F9:0</term>
///     <term>F9:0/1 或 F9:0.1</term>
///     <term>Floating point</term>
///   </item>
///   <item>
///     <term>S</term>
///     <term>S:0</term>
///     <term>S:0/1 或 S:0.1</term>
///     <term>Status  S:0 等同于 S2:0</term>
///   </item>
///   <item>
///     <term>ST</term>
///     <term>ST1:0</term>
///     <term></term>
///     <term>String</term>
///   </item>
///   <item>
///     <term>C</term>
///     <term>C9:0</term>
///     <term>C9:0/1 或 C9:0.1</term>
///     <term>Counter</term>
///   </item>
///   <item>
///     <term>I</term>
///     <term>I:0</term>
///     <term>I:0/1 或 I9:0.1</term>
///     <term>Input</term>
///   </item>
///   <item>
///     <term>O</term>
///     <term>O:0</term>
///     <term>O:0/1 或 O9:0.1</term>
///     <term>Output</term>
///   </item>
///   <item>
///     <term>R</term>
///     <term>R9:0</term>
///     <term>R9:0/1 或 R9:0.1</term>
///     <term>Control</term>
///   </item>
///   <item>
///     <term>T</term>
///     <term>T9:0</term>
///     <term>T9:0/1 或 T9:0.1</term>
///     <term>Timer</term>
///   </item>
///   <item>
///     <term>L</term>
///     <term>L9:0</term>
///     <term>L9:0/1 或 L9:0.1</term>
///     <term>long integer</term>
///   </item>
/// </list>
/// 感谢 seedee 的测试支持。
/// </example>
public class AllenBradleySLCNet : DeviceTcpNet
{
    /// <summary>
    /// The current session handle, which is determined by the PLC when communicating with the PLC handshake
    /// </summary>
    public uint SessionHandle { get; protected set; }

    /// <summary>
    /// Instantiate a communication object for a Allenbradley PLC protocol
    /// </summary>
    public AllenBradleySLCNet()
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// Instantiate a communication object for a Allenbradley PLC protocol
    /// </summary>
    /// <param name="ipAddress">PLC IpAddress</param>
    /// <param name="port">PLC Port</param>
    public AllenBradleySLCNet(string ipAddress, int port = 44818)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new AllenBradleySLCMessage();
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        var operateResult = ReadFromCoreServer(CommunicationPipe, "01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes(), hasResponseData: true, usePackAndUnpack: true);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        if (operateResult.Content.Length >= 8)
        {
            SessionHandle = ByteTransform.TransUInt32(operateResult.Content, 4);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(CommunicationPipe, "01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes(), hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (read.Content.Length >= 8)
        {
            SessionHandle = ByteTransform.TransUInt32(read.Content, 4);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// Read data information, data length for read array length information
    /// </summary>
    /// <param name="address">Address format of the node</param>
    /// <param name="length">In the case of arrays, the length of the array </param>
    /// <returns>Result data with result object </returns>
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        var operateResult = BuildReadCommand(address, length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = ExtraActualContent(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return operateResult3;
        }
        return OperateResult.CreateSuccessResult(operateResult3.Content);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        var operateResult = BuildWriteCommand(address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = ExtraActualContent(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return operateResult3;
        }
        return OperateResult.CreateSuccessResult(operateResult3.Content);
    }

    /// <inheritdoc />
    [HslMqttApi("ReadBool", "")]
    public override OperateResult<bool> ReadBool(string address)
    {
        address = AnalysisBitIndex(address, out var bitIndex);
        var operateResult = Read(address, 1);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content.ToBoolArray()[bitIndex]);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        var operateResult = BuildWriteCommand(address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommand(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = ExtraActualContent(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return operateResult3;
        }
        return OperateResult.CreateSuccessResult(operateResult3.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleySLCNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommand(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = ExtraActualContent(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult(extra.Content);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        var command = BuildWriteCommand(address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommand(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = ExtraActualContent(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult(extra.Content);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        address = AnalysisBitIndex(address, out var bitIndex);
        var read = await ReadAsync(address, 1);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray()[bitIndex]);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        var command = BuildWriteCommand(address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommand(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = ExtraActualContent(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult(extra.Content);
    }

    private byte[] PackCommand(byte[] coreCmd)
    {
        var array = new byte[28 + coreCmd.Length];
        array[0] = 1;
        array[1] = 7;
        array[2] = (byte)(coreCmd.Length / 256);
        array[3] = (byte)(coreCmd.Length % 256);
        BitConverter.GetBytes(SessionHandle).CopyTo(array, 4);
        coreCmd.CopyTo(array, 28);
        return array;
    }

    /// <summary>
    /// 分析地址数据信息里的位索引的信息
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <param name="bitIndex">位索引</param>
    /// <returns>地址信息</returns>
    public static string AnalysisBitIndex(string address, out int bitIndex)
    {
        bitIndex = 0;
        var num = address.IndexOf('/');
        if (num < 0)
        {
            num = address.IndexOf('.');
        }
        if (num > 0)
        {
            bitIndex = int.Parse(address.Substring(num + 1));
            address = address.Substring(0, num);
        }
        return address;
    }

    /// <summary>
    /// 构建读取的指令信息
    /// </summary>
    /// <param name="address">地址信息，举例：A9:0</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>是否成功</returns>
    public static OperateResult<byte[]> BuildReadCommand(string address, int length)
    {
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        if (length < 2)
        {
            length = 2;
        }
        if (operateResult.Content.DataCode == 142)
        {
            operateResult.Content.AddressStart /= 2;
        }
        var array = new byte[14]
        {
            0,
            5,
            0,
            0,
            15,
            0,
            0,
            1,
            162,
            (byte)length,
            (byte)operateResult.Content.DbBlock,
            operateResult.Content.DataCode,
            0,
            0
        };
        BitConverter.GetBytes((ushort)operateResult.Content.AddressStart).CopyTo(array, 12);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 构建写入的报文内容，变成实际的数据
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="value">数据值</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value)
    {
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        if (operateResult.Content.DataCode == 142)
        {
            operateResult.Content.AddressStart /= 2;
        }
        var array = new byte[18 + value.Length];
        array[0] = 0;
        array[1] = 5;
        array[2] = 0;
        array[3] = 0;
        array[4] = 15;
        array[5] = 0;
        array[6] = 0;
        array[7] = 1;
        array[8] = 171;
        array[9] = byte.MaxValue;
        array[10] = BitConverter.GetBytes(value.Length)[0];
        array[11] = BitConverter.GetBytes(value.Length)[1];
        array[12] = (byte)operateResult.Content.DbBlock;
        array[13] = operateResult.Content.DataCode;
        BitConverter.GetBytes((ushort)operateResult.Content.AddressStart).CopyTo(array, 14);
        array[16] = byte.MaxValue;
        array[17] = byte.MaxValue;
        value.CopyTo(array, 18);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 构建写入的报文内容，变成实际的数据
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="value">数据值</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildWriteCommand(string address, bool value)
    {
        address = AnalysisBitIndex(address, out var bitIndex);
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        if (operateResult.Content.DataCode == 142)
        {
            operateResult.Content.AddressStart /= 2;
        }
        var value2 = 1 << bitIndex;
        var array = new byte[20]
        {
            0,
            5,
            0,
            0,
            15,
            0,
            0,
            1,
            171,
            255,
            2,
            0,
            (byte)operateResult.Content.DbBlock,
            operateResult.Content.DataCode,
            0,
            0,
            0,
            0,
            0,
            0
        };
        BitConverter.GetBytes((ushort)operateResult.Content.AddressStart).CopyTo(array, 14);
        array[16] = BitConverter.GetBytes(value2)[0];
        array[17] = BitConverter.GetBytes(value2)[1];
        if (value)
        {
            array[18] = BitConverter.GetBytes(value2)[0];
            array[19] = BitConverter.GetBytes(value2)[1];
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 解析当前的实际报文内容，变成数据内容
    /// </summary>
    /// <param name="content">报文内容</param>
    /// <returns>是否成功</returns>
    public static OperateResult<byte[]> ExtraActualContent(byte[] content)
    {
        if (content.Length < 36)
        {
            return new OperateResult<byte[]>(StringResources.Language.ReceiveDataLengthTooShort + content.ToHexString(' '));
        }
        return OperateResult.CreateSuccessResult(content.RemoveBegin(36));
    }
}
