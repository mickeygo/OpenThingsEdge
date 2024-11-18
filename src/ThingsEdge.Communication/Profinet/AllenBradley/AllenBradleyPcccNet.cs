using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// 在CIP协议里，使用PCCC命令进行访问设备的原始数据文件的通信方法，
/// </summary>
/// <remarks>
/// <inheritdoc cref="T:HslCommunication.Profinet.AllenBradley.AllenBradleySLCNet" path="remarks" />
/// </remarks>
/// <example>
/// <inheritdoc cref="T:HslCommunication.Profinet.AllenBradley.AllenBradleySLCNet" path="example" />
/// </example>
public class AllenBradleyPcccNet : NetworkConnectedCip
{
    private SoftIncrementCount incrementCount = new SoftIncrementCount(65535L, 2L, 2);

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public AllenBradleyPcccNet()
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 根据指定的IP及端口来实例化这个连接对象
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口号信息</param>
    public AllenBradleyPcccNet(string ipAddress, int port = 44818)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override byte[] GetLargeForwardOpen(ushort connectionID)
    {
        TOConnectionId = (uint)CommHelper.HslRandom.Next();
        var array = "\r\n00 00 00 00 0a 00 02 00 00 00 00 00 b2 00 30 00\r\n54 02 20 06 24 01 0a 05 00 00 00 00 e8 a3 14 00\r\n27 04 09 10 0b 46 a5 c1 07 00 00 00 01 40 20 00\r\nf4 43 01 40 20 00 f4 43 a3 03 01 00 20 02 24 01".ToHexBytes();
        BitConverter.GetBytes((ushort)4105).CopyTo(array, 34);
        BitConverter.GetBytes(3248834059u).CopyTo(array, 36);
        BitConverter.GetBytes(TOConnectionId).CopyTo(array, 28);
        return array;
    }

    /// <inheritdoc />
    protected override byte[] GetLargeForwardClose()
    {
        return "\r\n00 00 00 00 0a 00 02 00 00 00 00 00 b2 00 18 00\r\n4e 02 20 06 24 01 0a 05 27 04 09 10 0b 46 a5 c1\r\n03 00 01 00 20 02 24 01".ToHexBytes();
    }

    /// <inheritdoc />
    /// <remarks>
    /// 读取PLC的原始数据信息，地址示例：N7:0
    /// </remarks>
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        var operateResult = AllenBradleyHelper.PackExecutePCCCRead((int)incrementCount.GetCurrentValue(), address, length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommandService(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        var operateResult4 = ExtractActualData(operateResult2.Content, isRead: true);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult4);
        }
        return OperateResult.CreateSuccessResult(operateResult4.Content1);
    }

    /// <inheritdoc />
    /// <remarks>
    /// 写入PLC的原始数据信息，地址示例：N7:0
    /// </remarks>
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        var operateResult = AllenBradleyHelper.PackExecutePCCCWrite((int)incrementCount.GetCurrentValue(), address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommandService(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        var operateResult4 = ExtractActualData(operateResult2.Content, isRead: true);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult4);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    [HslMqttApi("ReadBool", "")]
    public override OperateResult<bool> ReadBool(string address)
    {
        address = AllenBradleySLCNet.AnalysisBitIndex(address, out var bitIndex);
        var operateResult = Read(address, (ushort)(bitIndex / 16 * 2 + 2));
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content.ToBoolArray()[bitIndex]);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean)" />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        address = AllenBradleySLCNet.AnalysisBitIndex(address, out var bitIndex);
        var operateResult = AllenBradleyHelper.PackExecutePCCCWrite((int)incrementCount.GetCurrentValue(), address, bitIndex, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(PackCommandService(operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        var operateResult4 = ExtractActualData(operateResult2.Content, isRead: true);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult4);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Read(System.String,System.UInt16)" />
    public OperateResult<string> ReadString(string address)
    {
        return ReadString(address, 0, Encoding.ASCII);
    }

    /// <inheritdoc />
    /// <remarks>
    /// 读取PLC的地址信息，如果输入了 ST 的地址，例如 ST10:2, 当长度指定为 0 的时候，这时候就是动态的读取PLC来获取实际的字符串长度。<br />
    /// Read the PLC address information, if the ST address is entered, such as ST10:2, when the length is specified as 0, then the PLC is dynamically read to obtain the actual string length.
    /// </remarks>
    public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
    {
        if (!string.IsNullOrEmpty(address) && address.StartsWith("ST"))
        {
            if (length <= 0)
            {
                var operateResult = Read(address, 2);
                if (!operateResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<string>(operateResult);
                }
                int num = ByteTransform.TransUInt16(operateResult.Content, 0);
                operateResult = Read(address, (ushort)(2 + (num % 2 != 0 ? num + 1 : num)));
                if (!operateResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<string>(operateResult);
                }
                return OperateResult.CreateSuccessResult(encoding.GetString(SoftBasic.BytesReverseByWord(operateResult.Content), 2, num));
            }
            var operateResult2 = Read(address, (ushort)(length % 2 != 0 ? length + 3 : length + 2));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(operateResult2);
            }
            int num2 = ByteTransform.TransUInt16(operateResult2.Content, 0);
            if (num2 + 2 > operateResult2.Content.Length)
            {
                num2 = operateResult2.Content.Length - 2;
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(SoftBasic.BytesReverseByWord(operateResult2.Content), 2, num2));
        }
        return base.ReadString(address, length, encoding);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, string value, Encoding encoding)
    {
        if (!string.IsNullOrEmpty(address) && address.StartsWith("ST"))
        {
            var array = ByteTransform.TransByte(value, encoding);
            var value2 = array.Length;
            array = SoftBasic.ArrayExpandToLengthEven(array);
            return Write(address, SoftBasic.SpliceArray(new byte[2]
            {
                BitConverter.GetBytes(value2)[0],
                BitConverter.GetBytes(value2)[1]
            }, SoftBasic.BytesReverseByWord(array)));
        }
        return base.Write(address, value, encoding);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = AllenBradleyHelper.PackExecutePCCCRead((int)incrementCount.GetCurrentValue(), address, length);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommandService(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        var extra = ExtractActualData(read.Content, isRead: true);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(extra);
        }
        return OperateResult.CreateSuccessResult(extra.Content1);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        var command = AllenBradleyHelper.PackExecutePCCCWrite((int)incrementCount.GetCurrentValue(), address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommandService(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        var extra = ExtractActualData(read.Content, isRead: true);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(extra);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.ReadBool(System.String)" />
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        address = AllenBradleySLCNet.AnalysisBitIndex(address, out var bitIndex);
        var read = await ReadAsync(address, (ushort)(bitIndex / 16 * 2 + 2));
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray()[bitIndex]);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        address = AllenBradleySLCNet.AnalysisBitIndex(address, out var bitIndex);
        var command = AllenBradleyHelper.PackExecutePCCCWrite((int)incrementCount.GetCurrentValue(), address, bitIndex, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(PackCommandService(command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        var extra = ExtractActualData(read.Content, isRead: true);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(extra);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Read(System.String,System.UInt16)" />
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return await ReadStringAsync(address, 0, Encoding.ASCII);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.ReadString(System.String,System.UInt16,System.Text.Encoding)" />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        if (!string.IsNullOrEmpty(address) && address.StartsWith("ST"))
        {
            OperateResult<byte[]> read;
            int len;
            if (length <= 0)
            {
                read = await ReadAsync(address, 2);
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<string>(read);
                }
                len = ByteTransform.TransUInt16(read.Content, 0);
                read = await ReadAsync(address, (ushort)(2 + (len % 2 != 0 ? len + 1 : len)));
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<string>(read);
                }
                return OperateResult.CreateSuccessResult(encoding.GetString(SoftBasic.BytesReverseByWord(read.Content), 2, len));
            }
            read = await ReadAsync(address, (ushort)(length % 2 != 0 ? length + 3 : length + 2));
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read);
            }
            len = ByteTransform.TransUInt16(read.Content, 0);
            if (len + 2 > read.Content.Length)
            {
                len = read.Content.Length - 2;
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(SoftBasic.BytesReverseByWord(read.Content), 2, len));
        }
        return base.ReadString(address, length, encoding);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (!string.IsNullOrEmpty(address) && address.StartsWith("ST"))
        {
            var temp = ByteTransform.TransByte(value, encoding);
            var len = temp.Length;
            temp = SoftBasic.ArrayExpandToLengthEven(temp);
            return await WriteAsync(address, SoftBasic.SpliceArray(new byte[2]
            {
                BitConverter.GetBytes(len)[0],
                BitConverter.GetBytes(len)[1]
            }, SoftBasic.BytesReverseByWord(temp)));
        }
        return await base.WriteAsync(address, value, encoding);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByte", "")]
    public OperateResult<byte> ReadByte(string address)
    {
        return ByteTransformHelper.GetResultFromArray(Read(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Write(System.String,System.Byte)" />
    [HslMqttApi("WriteByte", "")]
    public OperateResult Write(string address, byte value)
    {
        return Write(address, new byte[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.ReadByte(System.String)" />
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyPcccNet.Write(System.String,System.Byte)" />
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteAsync(address, new byte[1] { value });
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AllenBradleyPcccNet[{IpAddress}:{Port}]";
    }
}
