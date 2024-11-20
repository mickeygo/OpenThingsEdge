using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Serial;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AB-PLC的DF1通信协议，基于串口实现，通信机制为半双工，目前适用于 Micro-Logix1000,SLC500,SLC 5/03,SLC 5/04，地址示例：N7:1
/// </summary>
public class AllenBradleyDF1Serial : DeviceSerialPort
{
    private readonly SoftIncrementCount _incrementCount = new(65535L, 0L);

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte Station { get; set; }

    /// <summary>
    /// 目标节点号
    /// </summary>
    public byte DstNode { get; set; }

    /// <summary>
    /// 源节点号
    /// </summary>
    public byte SrcNode { get; set; }

    /// <summary>
    /// 校验方式
    /// </summary>
    public CheckType CheckType { get; set; }

    /// <summary>
    /// Instantiate a communication object for a Allenbradley PLC protocol
    /// </summary>
    public AllenBradleyDF1Serial()
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
        CheckType = CheckType.CRC16;
    }

    /// <summary>
    /// 读取PLC的原始数据信息，地址示例：N7:0  可以携带站号 s=2;N7:0, 携带 dst 和 src 信息，例如 dst=1;src=2;N7:0
    /// </summary>
    /// <param name="address">PLC的地址信息，支持的类型见类型注释说明</param>
    /// <param name="length">读取的长度，单位，字节</param>
    /// <returns>是否读取成功的结果对象</returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", Station);
        var dstNode = (byte)CommunicationHelper.ExtractParameter(ref address, "dst", DstNode);
        var srcNode = (byte)CommunicationHelper.ExtractParameter(ref address, "src", SrcNode);
        var operateResult = BuildProtectedTypedLogicalReadWithThreeAddressFields(dstNode, srcNode, (int)_incrementCount.GetCurrentValue(), address, length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = await ReadFromCoreServerAsync(PackCommand(station, operateResult.Content)).ConfigureAwait(false);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return ExtractActualData(operateResult2.Content);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 写入PLC的原始数据信息，地址示例：N7:0  可以携带站号 s=2;N7:0, 携带 dst 和 src 信息，例如 dst=1;src=2;N7:0
    /// </summary>
    /// <param name="address">PLC的地址信息，支持的类型见类型注释说明</param>
    /// <param name="values">原始的数据值</param>
    /// <returns>是否写入成功</returns>
    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        var station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", Station);
        var dstNode = (byte)CommunicationHelper.ExtractParameter(ref address, "dst", DstNode);
        var srcNode = (byte)CommunicationHelper.ExtractParameter(ref address, "src", SrcNode);
        var operateResult = BuildProtectedTypedLogicalWriteWithThreeAddressFields(dstNode, srcNode, (int)_incrementCount.GetCurrentValue(), address, values);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = await ReadFromCoreServerAsync(PackCommand(station, operateResult.Content)).ConfigureAwait(false);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return ExtractActualData(operateResult2.Content);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        throw new NotImplementedException();
    }

    private byte[] CalculateCheckResult(byte station, byte[] command)
    {
        if (CheckType == CheckType.BCC)
        {
            int num = station;
            for (var i = 0; i < command.Length; i++)
            {
                num += command[i];
            }
            num = (byte)~num;
            num++;
            return [(byte)num];
        }
        var value = SoftBasic.SpliceArray([station], new byte[1] { 2 }, command, new byte[1] { 3 });
        return SoftCRC16.CRC16(value, 160, 1, 0, 0).SelectLast(2);
    }

    /// <summary>
    /// 打包命令的操作，加站号进行打包成完整的数据内容，命令内容为原始命令，打包后会自动补充0x10的值
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="command">等待发送的命令</param>
    /// <returns>打包之后的数据内容</returns>
    private byte[] PackCommand(byte station, byte[] command)
    {
        var array = CalculateCheckResult(station, command);
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(16);
        memoryStream.WriteByte(1);
        memoryStream.WriteByte(station);
        if (station == 16)
        {
            memoryStream.WriteByte(station);
        }
        memoryStream.WriteByte(16);
        memoryStream.WriteByte(2);
        for (var i = 0; i < command.Length; i++)
        {
            memoryStream.WriteByte(command[i]);
            if (command[i] == 16)
            {
                memoryStream.WriteByte(command[i]);
            }
        }
        memoryStream.WriteByte(16);
        memoryStream.WriteByte(3);
        memoryStream.Write(array, 0, array.Length);
        return memoryStream.ToArray();
    }

    private static void AddLengthToMemoryStream(MemoryStream ms, ushort value)
    {
        if (value < 255)
        {
            ms.WriteByte((byte)value);
            return;
        }
        ms.WriteByte(byte.MaxValue);
        ms.WriteByte(BitConverter.GetBytes(value)[0]);
        ms.WriteByte(BitConverter.GetBytes(value)[1]);
    }

    public static OperateResult<byte[]> BuildProtectedTypedLogicalReadWithThreeAddressFields(int tns, string address, int length)
    {
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(15);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[1]);
        memoryStream.WriteByte(162);
        memoryStream.WriteByte(BitConverter.GetBytes(length)[0]);
        AddLengthToMemoryStream(memoryStream, operateResult.Content.DbBlock);
        memoryStream.WriteByte(operateResult.Content.DataCode);
        AddLengthToMemoryStream(memoryStream, (ushort)operateResult.Content.AddressStart);
        AddLengthToMemoryStream(memoryStream, 0);
        return OperateResult.CreateSuccessResult(memoryStream.ToArray());
    }

    /// <summary>
    /// 构建0F-A2命令码的报文读取指令，用来读取文件数据。适用 Micro-Logix1000,SLC500,SLC 5/03,SLC 5/04, PLC-5，地址示例：N7:1。
    /// </summary>
    /// <param name="dstNode">目标节点号</param>
    /// <param name="srcNode">原节点号</param>
    /// <param name="tns">消息号</param>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>初步的报文信息</returns>
    /// <remarks>
    /// 对于SLC 5/01或SLC 5/02而言，一次最多读取82个字节。对于 03 或是 04 为225，236字节取决于是否应用DF1驱动。
    /// </remarks>
    public static OperateResult<byte[]> BuildProtectedTypedLogicalReadWithThreeAddressFields(byte dstNode, byte srcNode, int tns, string address, int length)
    {
        var operateResult = BuildProtectedTypedLogicalReadWithThreeAddressFields(tns, address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray([dstNode, srcNode], operateResult.Content));
    }

    public static OperateResult<byte[]> BuildProtectedTypedLogicalWriteWithThreeAddressFields(int tns, string address, byte[] data)
    {
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(15);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[1]);
        memoryStream.WriteByte(170);
        memoryStream.WriteByte(BitConverter.GetBytes(data.Length)[0]);
        AddLengthToMemoryStream(memoryStream, operateResult.Content.DbBlock);
        memoryStream.WriteByte(operateResult.Content.DataCode);
        AddLengthToMemoryStream(memoryStream, (ushort)operateResult.Content.AddressStart);
        AddLengthToMemoryStream(memoryStream, 0);
        memoryStream.Write(data);
        return OperateResult.CreateSuccessResult(memoryStream.ToArray());
    }

    /// <summary>
    /// 构建0F-AA命令码的写入读取指令，用来写入文件数据。适用 Micro-Logix1000,SLC500,SLC 5/03,SLC 5/04, PLC-5，地址示例：N7:1。
    /// </summary>
    /// <param name="dstNode">目标节点号</param>
    /// <param name="srcNode">原节点号</param>
    /// <param name="tns">消息号</param>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="data">写入的数据内容</param>
    /// <returns>初步的报文信息</returns>
    /// <remarks>
    /// 对于SLC 5/01或SLC 5/02而言，一次最多读取82个字节。对于 03 或是 04 为225，236字节取决于是否应用DF1驱动。
    /// </remarks>
    public static OperateResult<byte[]> BuildProtectedTypedLogicalWriteWithThreeAddressFields(byte dstNode, byte srcNode, int tns, string address, byte[] data)
    {
        var operateResult = BuildProtectedTypedLogicalWriteWithThreeAddressFields(tns, address, data);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray([dstNode, srcNode], operateResult.Content));
    }

    /// <summary>
    /// 构建0F-AB的掩码写入的功能
    /// </summary>
    /// <param name="tns">消息号</param>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="bitIndex">位索引信息</param>
    /// <param name="value">通断值</param>
    /// <returns>命令报文</returns>
    public static OperateResult<byte[]> BuildProtectedTypedLogicalMaskWithThreeAddressFields(int tns, string address, int bitIndex, bool value)
    {
        var value2 = 1 << bitIndex;
        var operateResult = AllenBradleySLCAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(15);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(tns)[1]);
        memoryStream.WriteByte(171);
        memoryStream.WriteByte(2);
        AddLengthToMemoryStream(memoryStream, operateResult.Content.DbBlock);
        memoryStream.WriteByte(operateResult.Content.DataCode);
        AddLengthToMemoryStream(memoryStream, (ushort)operateResult.Content.AddressStart);
        AddLengthToMemoryStream(memoryStream, 0);
        memoryStream.WriteByte(BitConverter.GetBytes(value2)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(value2)[1]);
        if (value)
        {
            memoryStream.WriteByte(BitConverter.GetBytes(value2)[0]);
            memoryStream.WriteByte(BitConverter.GetBytes(value2)[1]);
        }
        else
        {
            memoryStream.WriteByte(0);
            memoryStream.WriteByte(0);
        }
        return OperateResult.CreateSuccessResult(memoryStream.ToArray());
    }

    /// <summary>
    /// 提取返回报文的数据内容，将其转换成实际的数据内容，如果PLC返回了错误信息，则结果对象为失败。
    /// </summary>
    /// <param name="content">PLC返回的报文信息</param>
    /// <returns>结果对象内容</returns>
    public static OperateResult<byte[]> ExtractActualData(byte[] content)
    {
        try
        {
            var num = -1;
            for (var i = 0; i < content.Length; i++)
            {
                if (content[i] == 16 && content[i + 1] == 2)
                {
                    num = i + 2;
                    break;
                }
            }
            if (num < 0 || num >= content.Length - 6)
            {
                return new OperateResult<byte[]>("Message must start with '10 02', source: " + content.ToHexString(' '));
            }

            var memoryStream = new MemoryStream();
            for (var j = num; j < content.Length - 1; j++)
            {
                if (content[j] == 16 && content[j + 1] == 16)
                {
                    memoryStream.WriteByte(content[j]);
                    j++;
                    continue;
                }
                if (content[j] == 16 && content[j + 1] == 3)
                {
                    break;
                }
                memoryStream.WriteByte(content[j]);
            }
            content = memoryStream.ToArray();
            if (content[3] == 240)
            {
                return new OperateResult<byte[]>(GetExtStatusDescription(content[6]));
            }
            if (content[3] != 0)
            {
                return new OperateResult<byte[]>(GetStatusDescription(content[3]));
            }
            if (content.Length > 6)
            {
                return OperateResult.CreateSuccessResult(content.RemoveBegin(6));
            }
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message + " Source:" + content.ToHexString(' '));
        }
    }

    /// <summary>
    /// 根据错误代码，来获取错误的具体描述文本
    /// </summary>
    /// <param name="code">错误的代码，非0</param>
    /// <returns>错误的描述文本信息</returns>
    public static string GetStatusDescription(byte code)
    {
        var b = (byte)(code & 0xFu);
        var b2 = (byte)(code & 0xF0u);
        return b switch
        {
            1 => "DST node is out of buffer space",
            2 => "Cannot guarantee delivery: link layer(The remote node specified does not ACK command.)",
            3 => "Duplicate token holder detected",
            4 => "Local port is disconnected",
            5 => "Application layer timed out waiting for a response",
            6 => "Duplicate node detected",
            7 => "Station is offline",
            8 => "Hardware fault",
            _ => b2 switch
            {
                16 => "Illegal command or format",
                32 => "Host has a problem and will not communicate",
                48 => "Remote node host is missing, disconnected, or shut down",
                64 => "Host could not complete function due to hardware fault",
                80 => "Addressing problem or memory protect rungs",
                96 => "Function not allowed due to command protection selection",
                112 => "Processor is in Program mode",
                128 => "Compatibility mode file missing or communication zone problem",
                144 => "Remote node cannot buffer command",
                160 => "Wait ACK (1775\u0006KA buffer full)",
                176 => "Remote node problem due to download",
                192 => "Wait ACK (1775\u0006KA buffer full)",
                240 => "Error code in the EXT STS byte",
                _ => StringResources.Language.UnknownError,
            },
        };
    }

    /// <summary>
    /// 根据错误代码，来获取错误的具体描述文本
    /// </summary>
    /// <param name="code">错误的代码，非0</param>
    /// <returns>错误的描述文本信息</returns>
    public static string GetExtStatusDescription(byte code)
    {
        return code switch
        {
            1 => "A field has an illegal value",
            2 => "Less levels specified in address than minimum for any address",
            3 => "More levels specified in address than system supports",
            4 => "Symbol not found",
            5 => "Symbol is of improper format",
            6 => "Address doesn’t point to something usable",
            7 => "File is wrong size",
            8 => "Cannot complete request, situation has changed since the start of the command",
            9 => "Data or file is too large",
            10 => "Transaction size plus word address is too large",
            11 => "Access denied, improper privilege",
            12 => "Condition cannot be generated \u0006 resource is not available",
            13 => "Condition already exists \u0006 resource is already available",
            14 => "Command cannot be executed",
            15 => "Histogram overflow",
            16 => "No access",
            17 => "Illegal data type",
            18 => "Invalid parameter or invalid data",
            19 => "Address reference exists to deleted area",
            20 => "Command execution failure for unknown reason; possible PLC\u00063 histogram overflow",
            21 => "Data conversion error",
            22 => "Scanner not able to communicate with 1771 rack adapter",
            23 => "Type mismatch",
            24 => "1771 module response was not valid",
            25 => "Duplicated label",
            26 => "File is open; another node owns it",
            27 => "Another node is the program owner",
            28 => "Reserved",
            29 => "Reserved",
            30 => "Data table element protection violation",
            31 => "Temporary internal problem",
            34 => "Remote rack fault",
            35 => "Timeout",
            36 => "Unknown error",
            _ => StringResources.Language.UnknownError,
        };
    }

    public override string ToString()
    {
        return $"AllenBradleyDF1Serial[{PortName}:{BaudRate}]";
    }
}
