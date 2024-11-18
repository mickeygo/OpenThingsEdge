using System.Globalization;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的辅助类，用来辅助生成基本的指令信息
/// </summary>
public static class AllenBradleyHelper
{
    /// <summary>
    /// CIP命令中PCCC命令相关的信息
    /// </summary>
    public const byte CIP_Execute_PCCC = 75;

    /// <summary>
    /// CIP命令中的读取数据的服务
    /// </summary>
    public const byte CIP_READ_DATA = 76;

    /// <summary>
    /// CIP命令中的写数据的服务
    /// </summary>
    public const int CIP_WRITE_DATA = 77;

    /// <summary>
    /// CIP命令中的读并写的数据服务
    /// </summary>
    public const int CIP_READ_WRITE_DATA = 78;

    /// <summary>
    /// CIP命令中的读片段的数据服务
    /// </summary>
    public const int CIP_READ_FRAGMENT = 82;

    /// <summary>
    /// CIP命令中的写片段的数据服务
    /// </summary>
    public const int CIP_WRITE_FRAGMENT = 83;

    /// <summary>
    /// CIP指令中读取数据的列表
    /// </summary>
    public const byte CIP_READ_LIST = 85;

    /// <summary>
    /// CIP命令中的对数据读取服务
    /// </summary>
    public const int CIP_MULTIREAD_DATA = 4096;

    /// <summary>
    /// 日期的格式
    /// </summary>
    public const ushort CIP_Type_DATE = 8;

    /// <summary>
    /// 时间的格式
    /// </summary>
    public const ushort CIP_Type_TIME = 9;

    /// <summary>
    /// 日期时间格式，最完整的时间格式
    /// </summary>
    public const ushort CIP_Type_TimeAndDate = 10;

    /// <summary>
    /// 一天中的时间格式
    /// </summary>
    public const ushort CIP_Type_TimeOfDate = 11;

    /// <summary>
    /// bool型数据，一个字节长度
    /// </summary>
    public const ushort CIP_Type_Bool = 193;

    /// <summary>
    /// byte型数据，一个字节长度，SINT
    /// </summary>
    public const ushort CIP_Type_Byte = 194;

    /// <summary>
    /// 整型，两个字节长度，INT
    /// </summary>
    public const ushort CIP_Type_Word = 195;

    /// <summary>
    /// 长整型，四个字节长度，DINT
    /// </summary>
    public const ushort CIP_Type_DWord = 196;

    /// <summary>
    /// 特长整型，8个字节，LINT
    /// </summary>
    public const ushort CIP_Type_LInt = 197;

    /// <summary>
    /// Unsigned 8-bit integer, USINT
    /// </summary>
    public const ushort CIP_Type_USInt = 198;

    /// <summary>
    /// Unsigned 16-bit integer, UINT
    /// </summary>
    public const ushort CIP_Type_UInt = 199;

    /// <summary>
    ///  Unsigned 32-bit integer, UDINT
    /// </summary>
    public const ushort CIP_Type_UDint = 200;

    /// <summary>
    ///  Unsigned 64-bit integer, ULINT
    /// </summary>
    public const ushort CIP_Type_ULint = 201;

    /// <summary>
    /// 实数数据，四个字节长度
    /// </summary>
    public const ushort CIP_Type_Real = 202;

    /// <summary>
    /// 实数数据，八个字节的长度
    /// </summary>
    public const ushort CIP_Type_Double = 203;

    /// <summary>
    /// 结构体数据，不定长度
    /// </summary>
    public const ushort CIP_Type_Struct = 204;

    /// <summary>
    /// 字符串数据内容
    /// </summary>
    public const ushort CIP_Type_String = 208;

    /// <summary>
    ///  Bit string, 8 bits, BYTE,
    /// </summary>
    public const ushort CIP_Type_D1 = 209;

    /// <summary>
    /// Bit string, 16-bits, WORD
    /// </summary>
    public const ushort CIP_Type_D2 = 210;

    /// <summary>
    /// Bit string, 32 bits, DWORD
    /// </summary>
    public const ushort CIP_Type_D3 = 211;

    /// <summary>
    /// Bit string, 64 bits LWORD
    /// </summary>
    public const ushort CIP_Type_D4 = 212;

    /// <summary>
    /// 二进制数据内容
    /// </summary>
    public const ushort CIP_Type_BitArray = 211;

    /// <summary>
    /// 连接方的厂商标识
    /// </summary>
    public const ushort OriginatorVendorID = 4105;

    /// <summary>
    /// 连接方的序列号
    /// </summary>
    public const uint OriginatorSerialNumber = 3248834059u;

    private static byte[] BuildRequestPathCommand(string address, bool isConnectedAddress = false)
    {
        using var memoryStream = new MemoryStream();
        var num = CommunicationHelper.ExtractParameter(ref address, "class", -1);
        if (num != -1)
        {
            var num2 = address.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(address.Substring(2), 16) : Convert.ToInt32(address);
            if (num < 256)
            {
                memoryStream.WriteByte(32);
                memoryStream.WriteByte((byte)num);
            }
            else
            {
                memoryStream.WriteByte(33);
                memoryStream.WriteByte(0);
                memoryStream.WriteByte(BitConverter.GetBytes(num)[0]);
                memoryStream.WriteByte(BitConverter.GetBytes(num)[1]);
            }
            if (num2 < 256)
            {
                memoryStream.WriteByte(36);
                memoryStream.WriteByte((byte)num2);
            }
            else
            {
                memoryStream.WriteByte(37);
                memoryStream.WriteByte(0);
                memoryStream.WriteByte(BitConverter.GetBytes(num2)[0]);
                memoryStream.WriteByte(BitConverter.GetBytes(num2)[1]);
            }
        }
        else
        {
            var array = address.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < array.Length; i++)
            {
                var text = string.Empty;
                var num3 = array[i].IndexOf('[');
                var num4 = array[i].IndexOf(']');
                if (num3 > 0 && num4 > 0 && num4 > num3)
                {
                    text = array[i].Substring(num3 + 1, num4 - num3 - 1);
                    array[i] = array[i].Substring(0, num3);
                }
                memoryStream.WriteByte(145);
                var bytes = Encoding.UTF8.GetBytes(array[i]);
                memoryStream.WriteByte((byte)bytes.Length);
                memoryStream.Write(bytes, 0, bytes.Length);
                if (bytes.Length % 2 == 1)
                {
                    memoryStream.WriteByte(0);
                }
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                var array2 = text.Split([','], StringSplitOptions.RemoveEmptyEntries);
                for (var j = 0; j < array2.Length; j++)
                {
                    var num5 = Convert.ToInt32(array2[j], CultureInfo.InvariantCulture);
                    if (num5 < 256 && !isConnectedAddress)
                    {
                        memoryStream.WriteByte(40);
                        memoryStream.WriteByte((byte)num5);
                    }
                    else if (num5 < 65536)
                    {
                        memoryStream.WriteByte(41);
                        memoryStream.WriteByte(0);
                        memoryStream.WriteByte(BitConverter.GetBytes(num5)[0]);
                        memoryStream.WriteByte(BitConverter.GetBytes(num5)[1]);
                    }
                    else
                    {
                        memoryStream.WriteByte(42);
                        memoryStream.WriteByte(0);
                        memoryStream.Write(BitConverter.GetBytes(num5));
                    }
                }
            }
        }
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 从生成的报文里面反解出实际的数据地址，不支持结构体嵌套，仅支持数据，一维数组，不支持多维数据。
    /// </summary>
    /// <param name="pathCommand">地址路径报文</param>
    /// <returns>实际的地址</returns>
    public static string ParseRequestPathCommand(byte[] pathCommand)
    {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < pathCommand.Length; i++)
        {
            if (pathCommand[i] != 145)
            {
                continue;
            }

            var text = Encoding.UTF8.GetString(pathCommand, i + 2, pathCommand[i + 1]).TrimEnd(default(char));
            stringBuilder.Append(text);
            var num = 2 + text.Length;
            if (text.Length % 2 == 1)
            {
                num++;
            }
            if (pathCommand.Length > num + i)
            {
                if (pathCommand[i + num] == 40)
                {
                    stringBuilder.Append($"[{pathCommand[i + num + 1]}]");
                }
                else if (pathCommand[i + num] == 41)
                {
                    stringBuilder.Append($"[{BitConverter.ToUInt16(pathCommand, i + num + 2)}]");
                }
            }
            stringBuilder.Append('.');
        }
        if (stringBuilder[stringBuilder.Length - 1] == '.')
        {
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 从生成的报文里面，反解出实际的 Symbol Class ID，以及  Instance ID 信息。
    /// </summary>
    /// <param name="pathCommand"></param>
    /// <returns></returns>
    public static OperateResult<int, int> ParseRequestPathSymbolInstanceAddressing(byte[] pathCommand)
    {
        var num3 = 0;
        int num;
        if (pathCommand[num3] == 32)
        {
            num3++;
            num = pathCommand[num3++];
        }
        else
        {
            if (pathCommand[num3] != 33)
            {
                return new OperateResult<int, int>();
            }
            num3 += 2;
            num = BitConverter.ToUInt16(pathCommand, num3);
            num3 += 2;
        }
        int num2;
        if (pathCommand[num3] == 36)
        {
            num3++;
            num2 = pathCommand[num3++];
        }
        else
        {
            if (pathCommand[num3] != 37)
            {
                return new OperateResult<int, int>();
            }
            num3 += 2;
            num2 = BitConverter.ToUInt16(pathCommand, num3);
        }
        return OperateResult.CreateSuccessResult(num, num2);
    }

    /// <summary>
    /// 获取枚举PLC数据信息的指令
    /// </summary>
    /// <param name="startInstance">实例的起始地址</param>
    /// <returns>结果数据</returns>
    public static byte[] BuildEnumeratorCommand(uint startInstance)
    {
        return
        [
            85,
            3,
            32,
            107,
            37,
            0,
            BitConverter.GetBytes(startInstance)[0],
            BitConverter.GetBytes(startInstance)[1],
            3,
            0,
            1,
            0,
            2,
            0,
            8,
            0
        ];
    }

    /// <summary>
    /// 获取枚举PLC的局部变量的数据信息的指令
    /// </summary>
    /// <param name="startInstance">实例的起始地址</param>
    /// <returns>结果命令数据</returns>
    public static byte[] BuildEnumeratorProgrameMainCommand(uint startInstance)
    {
        var array = new byte[38];
        array[0] = 85;
        array[1] = 14;
        array[2] = 145;
        array[3] = 19;
        Encoding.ASCII.GetBytes("Program:MainProgram").CopyTo(array, 4);
        array[23] = 0;
        array[24] = 32;
        array[25] = 107;
        array[26] = 37;
        array[27] = 0;
        array[28] = BitConverter.GetBytes(startInstance)[0];
        array[29] = BitConverter.GetBytes(startInstance)[1];
        array[30] = 3;
        array[31] = 0;
        array[32] = 1;
        array[33] = 0;
        array[34] = 2;
        array[35] = 0;
        array[36] = 8;
        array[37] = 0;
        return array;
    }

    /// <summary>
    /// 获取获得结构体句柄的命令
    /// </summary>
    /// <param name="symbolType">包含地址的信息</param>
    /// <returns>命令数据</returns>
    public static byte[] GetStructHandleCommand(ushort symbolType)
    {
        var array = new byte[18];
        symbolType = (ushort)(symbolType & 0xFFFu);
        array[0] = 3;
        array[1] = 3;
        array[2] = 32;
        array[3] = 108;
        array[4] = 37;
        array[5] = 0;
        array[6] = BitConverter.GetBytes(symbolType)[0];
        array[7] = BitConverter.GetBytes(symbolType)[1];
        array[8] = 4;
        array[9] = 0;
        array[10] = 4;
        array[11] = 0;
        array[12] = 5;
        array[13] = 0;
        array[14] = 2;
        array[15] = 0;
        array[16] = 1;
        array[17] = 0;
        return array;
    }

    /// <summary>
    /// 获取结构体内部数据结构的方法
    /// </summary>
    /// <param name="symbolType">地址</param>
    /// <param name="structHandle">句柄</param>
    /// <param name="offset">偏移量地址</param>
    /// <returns>指令</returns>
    public static byte[] GetStructItemNameType(ushort symbolType, AbStructHandle structHandle, int offset)
    {
        var array = new byte[14];
        symbolType = (ushort)(symbolType & 0xFFFu);
        var bytes = BitConverter.GetBytes(structHandle.TemplateObjectDefinitionSize * 4 - 21);
        array[0] = 76;
        array[1] = 3;
        array[2] = 32;
        array[3] = 108;
        array[4] = 37;
        array[5] = 0;
        array[6] = BitConverter.GetBytes(symbolType)[0];
        array[7] = BitConverter.GetBytes(symbolType)[1];
        array[8] = BitConverter.GetBytes(offset)[0];
        array[9] = BitConverter.GetBytes(offset)[1];
        array[10] = BitConverter.GetBytes(offset)[2];
        array[11] = BitConverter.GetBytes(offset)[3];
        array[12] = bytes[0];
        array[13] = bytes[1];
        return array;
    }

    public static byte[] PackRequestHeader(ushort command, uint session, byte[] commandSpecificData, byte[]? senderContext = null)
    {
        commandSpecificData ??= [];
        var array = new byte[commandSpecificData.Length + 24];
        Array.Copy(commandSpecificData, 0, array, 24, commandSpecificData.Length);
        BitConverter.GetBytes(command).CopyTo(array, 0);
        BitConverter.GetBytes(session).CopyTo(array, 4);
        senderContext?.CopyTo(array, 12);
        BitConverter.GetBytes((ushort)commandSpecificData.Length).CopyTo(array, 2);
        return array;
    }

    /// <summary>
    /// 将CommandSpecificData的命令，打包成可发送的数据指令
    /// </summary>
    /// <param name="command">实际的命令暗号</param>
    /// <param name="error">错误号信息</param>
    /// <param name="session">当前会话的id</param>
    /// <param name="commandSpecificData">CommandSpecificData命令</param>
    /// <returns>最终可发送的数据命令</returns>
    public static byte[] PackRequestHeader(ushort command, uint error, uint session, byte[] commandSpecificData)
    {
        var array = PackRequestHeader(command, session, commandSpecificData);
        BitConverter.GetBytes(error).CopyTo(array, 8);
        return array;
    }

    private static byte[] PackExecutePCCC(byte[] pccc)
    {
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(75);
        memoryStream.WriteByte(2);
        memoryStream.WriteByte(32);
        memoryStream.WriteByte(103);
        memoryStream.WriteByte(36);
        memoryStream.WriteByte(1);
        memoryStream.WriteByte(7);
        memoryStream.WriteByte(9);
        memoryStream.WriteByte(16);
        memoryStream.WriteByte(11);
        memoryStream.WriteByte(70);
        memoryStream.WriteByte(165);
        memoryStream.WriteByte(193);
        memoryStream.Write(pccc);
        var array = memoryStream.ToArray();
        BitConverter.GetBytes((ushort)4105).CopyTo(array, 7);
        BitConverter.GetBytes(3248834059u).CopyTo(array, 9);
        return array;
    }

    /// <summary>
    /// 打包一个PCCC的读取的命令报文
    /// </summary>
    /// <param name="tns">请求序号信息</param>
    /// <param name="address">请求的文件地址，地址示例：N7:1</param>
    /// <param name="length">请求的字节长度</param>
    /// <returns>PCCC的读取报文信息</returns>
    public static OperateResult<byte[]> PackExecutePCCCRead(int tns, string address, int length)
    {
        var operateResult = AllenBradleyDF1Serial.BuildProtectedTypedLogicalReadWithThreeAddressFields(tns, address, length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return OperateResult.CreateSuccessResult(PackExecutePCCC(operateResult.Content));
    }

    /// <summary>
    /// 打包一个PCCC的写入的命令报文
    /// </summary>
    /// <param name="tns">请求序号信息</param>
    /// <param name="address">请求的文件地址，地址示例：N7:1</param>
    /// <param name="value">写入的原始数据信息</param>
    /// <returns>PCCC的写入报文信息</returns>
    public static OperateResult<byte[]> PackExecutePCCCWrite(int tns, string address, byte[] value)
    {
        var operateResult = AllenBradleyDF1Serial.BuildProtectedTypedLogicalWriteWithThreeAddressFields(tns, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return OperateResult.CreateSuccessResult(PackExecutePCCC(operateResult.Content));
    }

    internal static OperateResult<byte[]> PackExecutePCCCWrite(int tns, string address, int bitIndex, bool value)
    {
        var operateResult = AllenBradleyDF1Serial.BuildProtectedTypedLogicalMaskWithThreeAddressFields(tns, address, bitIndex, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return OperateResult.CreateSuccessResult(PackExecutePCCC(operateResult.Content));
    }

    /// <summary>
    /// 打包生成一个请求读取数据的节点信息，CIP指令信息
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="length">指代数组的长度</param>
    /// <param name="isConnectedAddress">是否是连接模式下的地址，默认为false</param>
    /// <returns>CIP的指令信息</returns>
    public static byte[] PackRequsetRead(string address, int length, bool isConnectedAddress = false)
    {
        var array = new byte[1024];
        var num = 0;
        array[num++] = 76;
        num++;
        var array2 = BuildRequestPathCommand(address, isConnectedAddress);
        array2.CopyTo(array, num);
        num += array2.Length;
        array[1] = (byte)((num - 2) / 2);
        array[num++] = BitConverter.GetBytes(length)[0];
        array[num++] = BitConverter.GetBytes(length)[1];
        var array3 = new byte[num];
        Array.Copy(array, 0, array3, 0, num);
        return array3;
    }

    /// <summary>
    /// 打包生成一个请求读取数据片段的节点信息，CIP指令信息
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <param name="startIndex">起始的索引位置，以字节为单位</param>
    /// <param name="length">读取的数据长度，一次通讯总计490个字节</param>
    /// <returns>CIP的指令信息</returns>
    public static byte[] PackRequestReadSegment(string address, int startIndex, int length)
    {
        var array = new byte[1024];
        var num = 0;
        array[num++] = 82;
        num++;
        var array2 = BuildRequestPathCommand(address);
        array2.CopyTo(array, num);
        num += array2.Length;
        array[1] = (byte)((num - 2) / 2);
        array[num++] = BitConverter.GetBytes(length)[0];
        array[num++] = BitConverter.GetBytes(length)[1];
        array[num++] = BitConverter.GetBytes(startIndex)[0];
        array[num++] = BitConverter.GetBytes(startIndex)[1];
        array[num++] = BitConverter.GetBytes(startIndex)[2];
        array[num++] = BitConverter.GetBytes(startIndex)[3];
        var array3 = new byte[num];
        Array.Copy(array, 0, array3, 0, num);
        return array3;
    }

    /// <summary>
    /// 根据指定的数据和类型，生成对应的数据
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="typeCode">数据类型</param>
    /// <param name="value">字节值</param>
    /// <param name="length">如果节点为数组，就是数组长度</param>
    /// <param name="isConnectedAddress">是否为连接模式的地址</param>
    /// <returns>CIP的指令信息</returns>
    public static byte[] PackRequestWrite(string address, ushort typeCode, byte[] value, int length = 1, bool isConnectedAddress = false)
    {
        var array = new byte[1024];
        var num = 0;
        array[num++] = 77;
        num++;
        var array2 = BuildRequestPathCommand(address, isConnectedAddress);
        array2.CopyTo(array, num);
        num += array2.Length;
        array[1] = (byte)((num - 2) / 2);
        array[num++] = BitConverter.GetBytes(typeCode)[0];
        array[num++] = BitConverter.GetBytes(typeCode)[1];
        array[num++] = BitConverter.GetBytes(length)[0];
        array[num++] = BitConverter.GetBytes(length)[1];
        value ??= [];
        var array3 = new byte[value.Length + num];
        Array.Copy(array, 0, array3, 0, num);
        value.CopyTo(array3, num);
        return array3;
    }

    public static byte[] PackRequestWriteSegment(string address, ushort typeCode, byte[] value, int startIndex, int length = 1, bool isConnectedAddress = false)
    {
        var array = new byte[1024];
        var num = 0;
        array[num++] = 83;
        num++;
        var array2 = BuildRequestPathCommand(address, isConnectedAddress);
        array2.CopyTo(array, num);
        num += array2.Length;
        array[1] = (byte)((num - 2) / 2);
        array[num++] = BitConverter.GetBytes(typeCode)[0];
        array[num++] = BitConverter.GetBytes(typeCode)[1];
        array[num++] = BitConverter.GetBytes(length)[0];
        array[num++] = BitConverter.GetBytes(length)[1];
        array[num++] = BitConverter.GetBytes(startIndex)[0];
        array[num++] = BitConverter.GetBytes(startIndex)[1];
        array[num++] = BitConverter.GetBytes(startIndex)[2];
        array[num++] = BitConverter.GetBytes(startIndex)[3];
        value ??= [];
        var array3 = new byte[value.Length + num];
        Array.Copy(array, 0, array3, 0, num);
        value.CopyTo(array3, num);
        return array3;
    }

    /// <summary>
    /// 根据传入的地址，或掩码，和掩码来创建一个读-修改-写的请求报文信息
    /// </summary>
    /// <param name="address">标签的地址信息</param>
    /// <param name="orMask">或的掩码</param>
    /// <param name="andMask">和的掩码</param>
    /// <param name="isConnectedAddress">是否为连接模式的地址</param>
    /// <returns>打包之后的CIP指令信息</returns>
    public static byte[] PackRequestReadModifyWrite(string address, uint orMask, uint andMask, bool isConnectedAddress = false)
    {
        var array = new byte[1024];
        var num = 0;
        array[num++] = 78;
        num++;
        var array2 = BuildRequestPathCommand(address, isConnectedAddress);
        array2.CopyTo(array, num);
        num += array2.Length;
        array[1] = (byte)((num - 2) / 2);
        array[num++] = 4;
        array[num++] = 0;
        BitConverter.GetBytes(orMask).CopyTo(array, num);
        num += 4;
        BitConverter.GetBytes(andMask).CopyTo(array, num);
        num += 4;
        return array.SelectBegin(num);
    }

    public static byte[] PackRequestReadModifyWrite(string address, int index, bool value, bool isConnectedAddress = false)
    {
        address += $"[{index / 32}]";
        index %= 32;
        if (value)
        {
            var num = 1u;
            num <<= index;
            return PackRequestReadModifyWrite(address, num, uint.MaxValue, isConnectedAddress);
        }
        var num2 = 1u;
        num2 <<= index;
        num2 = ~num2;
        return PackRequestReadModifyWrite(address, 0u, num2, isConnectedAddress);
    }

    /// <summary>
    /// 分析地址数据信息里的位索引的信息，例如a[10]  返回 a 和 10 索引，如果没有指定索引，就默认为0
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <param name="arrayIndex">位索引</param>
    /// <returns>地址信息</returns>
    public static string AnalysisArrayIndex(string address, out int arrayIndex)
    {
        arrayIndex = 0;
        if (!address.EndsWith(']'))
        {
            return address;
        }
        var num = address.LastIndexOf('[');
        if (num < 0)
        {
            return address;
        }

        address = address.Remove(address.Length - 1);
        try
        {
            arrayIndex = int.Parse(address[(num + 1)..]);
            address = address[..num];
            return address;
        }
        catch
        {
            return address;
        }
    }

    /// <summary>
    /// 写入Bool数据的基本指令信息
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="value">值</param>
    /// <returns>报文信息</returns>
    public static byte[] PackRequestWrite(string address, bool value)
    {
        address = AnalysisArrayIndex(address, out var arrayIndex);
        return PackRequestReadModifyWrite(address, arrayIndex, value);
    }

    /// <summary>
    /// 将所有的cip指定进行打包操作。
    /// </summary>
    /// <param name="portSlot">PLC所在的面板槽号</param>
    /// <param name="cips">所有的cip打包指令信息</param>
    /// <returns>包含服务的信息</returns>
    public static byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
    {
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(178);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(82);
        memoryStream.WriteByte(2);
        memoryStream.WriteByte(32);
        memoryStream.WriteByte(6);
        memoryStream.WriteByte(36);
        memoryStream.WriteByte(1);
        memoryStream.WriteByte(10);
        memoryStream.WriteByte(240);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        var num = 0;
        if (cips.Length == 1)
        {
            memoryStream.Write(cips[0], 0, cips[0].Length);
            num += cips[0].Length;
            if (cips[0].Length % 2 == 1)
            {
                memoryStream.WriteByte(0);
            }
        }
        else
        {
            memoryStream.WriteByte(10);
            memoryStream.WriteByte(2);
            memoryStream.WriteByte(32);
            memoryStream.WriteByte(2);
            memoryStream.WriteByte(36);
            memoryStream.WriteByte(1);
            num += 8;
            memoryStream.Write(BitConverter.GetBytes((ushort)cips.Length), 0, 2);
            var num2 = (ushort)(2 + 2 * cips.Length);
            num += 2 * cips.Length;
            for (var i = 0; i < cips.Length; i++)
            {
                memoryStream.Write(BitConverter.GetBytes(num2), 0, 2);
                num2 = (ushort)(num2 + cips[i].Length);
            }
            for (var j = 0; j < cips.Length; j++)
            {
                memoryStream.Write(cips[j], 0, cips[j].Length);
                num += cips[j].Length;
            }
        }
        if (portSlot != null)
        {
            memoryStream.WriteByte((byte)((portSlot.Length + 1) / 2));
            memoryStream.WriteByte(0);
            memoryStream.Write(portSlot, 0, portSlot.Length);
            if (portSlot.Length % 2 == 1)
            {
                memoryStream.WriteByte(0);
            }
        }
        var array = memoryStream.ToArray();
        BitConverter.GetBytes((short)num).CopyTo(array, 12);
        BitConverter.GetBytes((short)(array.Length - 4)).CopyTo(array, 2);
        return array;
    }

    /// <summary>
    /// 将所有的cip指定进行打包操作。
    /// </summary>
    /// <param name="portSlot">PLC所在的面板槽号</param>
    /// <param name="cips">所有的cip打包指令信息</param>
    /// <returns>包含服务的信息</returns>
    public static byte[] PackCleanCommandService(byte[] portSlot, params byte[][] cips)
    {
        using var memoryStream = new MemoryStream();
        memoryStream.WriteByte(178);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        if (cips.Length == 1)
        {
            memoryStream.Write(cips[0], 0, cips[0].Length);
        }
        else
        {
            memoryStream.WriteByte(10);
            memoryStream.WriteByte(2);
            memoryStream.WriteByte(32);
            memoryStream.WriteByte(2);
            memoryStream.WriteByte(36);
            memoryStream.WriteByte(1);
            memoryStream.Write(BitConverter.GetBytes((ushort)cips.Length), 0, 2);
            var num = (ushort)(2 + 2 * cips.Length);
            for (var i = 0; i < cips.Length; i++)
            {
                memoryStream.Write(BitConverter.GetBytes(num), 0, 2);
                num = (ushort)(num + cips[i].Length);
            }
            for (var j = 0; j < cips.Length; j++)
            {
                memoryStream.Write(cips[j], 0, cips[j].Length);
            }
        }
        memoryStream.WriteByte((byte)((portSlot.Length + 1) / 2));
        memoryStream.WriteByte(0);
        memoryStream.Write(portSlot, 0, portSlot.Length);
        if (portSlot.Length % 2 == 1)
        {
            memoryStream.WriteByte(0);
        }
        var array = memoryStream.ToArray();
        BitConverter.GetBytes((short)(array.Length - 4)).CopyTo(array, 2);
        return array;
    }

    /// <summary>
    /// 打包一个读取所有特性数据的报文信息，需要传入slot
    /// </summary>
    /// <param name="portSlot">站号信息</param>
    /// <param name="sessionHandle">会话的ID信息</param>
    /// <returns>最终发送的报文数据</returns>
    public static byte[] PackCommandGetAttributesAll(byte[] portSlot, uint sessionHandle)
    {
        var commandSpecificData = PackCommandSpecificData(new byte[4], PackCommandService(portSlot, [1, 2, 32, 1, 36, 1]));
        return PackRequestHeader(111, sessionHandle, commandSpecificData);
    }

    /// <summary>
    /// 根据数据创建反馈的数据信息
    /// </summary>
    /// <param name="data">反馈的数据信息</param>
    /// <param name="isRead">是否是读取</param>
    /// <returns>数据</returns>
    public static byte[] PackCommandResponse(byte[] data, bool isRead)
    {
        if (data == null)
        {
            return [0, 0, 4, 0, 0, 0];
        }
        return SoftBasic.SpliceArray(new byte[6]
        {
            (byte)(isRead ? 204u : 205u),
            0,
            0,
            0,
            0,
            0
        }, data);
    }

    /// <summary>
    /// 生成读取直接节点数据信息的内容
    /// </summary>
    /// <param name="service">cip指令内容</param>
    /// <returns>最终的指令值</returns>
    public static byte[] PackCommandSpecificData(params byte[][] service)
    {
        using var memoryStream = new MemoryStream();
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(10);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(service.Length)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(service.Length)[1]);
        for (var i = 0; i < service.Length; i++)
        {
            memoryStream.Write(service[i], 0, service[i].Length);
        }
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 将所有的cip指定进行打包操作。
    /// </summary>
    /// <param name="command">指令信息</param>
    /// <param name="code">服务器的代号信息</param>
    /// <param name="isConnected">是否基于连接的服务</param>
    /// <param name="sequence">当使用了基于连接的服务时，当前CIP消息号信息</param>
    /// <returns>包含服务的信息</returns>
    public static byte[] PackCommandSingleService(byte[] command, ushort code = 178, bool isConnected = false, ushort sequence = 0)
    {
        command ??= [];
        var array = isConnected ? new byte[6 + command.Length] : new byte[4 + command.Length];
        array[0] = BitConverter.GetBytes(code)[0];
        array[1] = BitConverter.GetBytes(code)[1];
        array[2] = BitConverter.GetBytes(array.Length - 4)[0];
        array[3] = BitConverter.GetBytes(array.Length - 4)[1];
        command.CopyTo(array, isConnected ? 6 : 4);
        if (isConnected)
        {
            BitConverter.GetBytes(sequence).CopyTo(array, 4);
        }
        return array;
    }

    /// <summary>
    /// 向PLC注册会话ID的报文。
    /// </summary>
    /// <param name="senderContext">发送的上下文信息</param>
    /// <returns>报文信息 </returns>
    public static byte[] RegisterSessionHandle(byte[]? senderContext = null)
    {
        var commandSpecificData = new byte[4] { 1, 0, 0, 0 };
        return PackRequestHeader(101, 0u, commandSpecificData, senderContext);
    }

    /// <summary>
    /// 获取卸载一个已注册的会话的报文。
    /// </summary>
    /// <param name="sessionHandle">当前会话的ID信息</param>
    /// <returns>字节报文信息</returns>
    public static byte[] UnRegisterSessionHandle(uint sessionHandle)
    {
        return PackRequestHeader(102, sessionHandle, []);
    }

    /// <summary>
    /// 初步检查返回的CIP协议的报文是否正确。
    /// </summary>
    /// <param name="response">CIP的报文信息</param>
    /// <returns>是否正确的结果信息</returns>
    public static OperateResult CheckResponse(byte[] response)
    {
        try
        {
            var num = BitConverter.ToInt32(response, 8);
            if (num == 0)
            {
                return OperateResult.CreateSuccessResult();
            }

            var empty = string.Empty;
            return new OperateResult(num, num switch
            {
                1 => StringResources.Language.AllenBradleySessionStatus01,
                2 => StringResources.Language.AllenBradleySessionStatus02,
                3 => StringResources.Language.AllenBradleySessionStatus03,
                100 => StringResources.Language.AllenBradleySessionStatus64,
                101 => StringResources.Language.AllenBradleySessionStatus65,
                105 => StringResources.Language.AllenBradleySessionStatus69,
                _ => StringResources.Language.UnknownError,
            });
        }
        catch (Exception ex)
        {
            return new OperateResult("CheckResponse failed: " + ex.Message + Environment.NewLine + "Source: " + response.ToHexString(' '));
        }
    }

    /// <summary>
    /// 从PLC反馈的数据解析，返回解析后的数据内容，数据类型（在多项数据返回中无效），以及是否有更多的数据
    /// </summary>
    /// <param name="response">PLC的反馈数据</param>
    /// <param name="isRead">是否是返回的操作</param>
    /// <returns>带有结果标识的最终数据</returns>
    public static OperateResult<byte[], ushort, bool> ExtractActualData(byte[] response, bool isRead)
    {
        var list = new List<byte>();
        try
        {
            var num = 38;
            var value = false;
            ushort value2 = 0;
            var num2 = BitConverter.ToUInt16(response, 38);
            if (BitConverter.ToInt32(response, 40) == 138)
            {
                num = 44;
                int num3 = BitConverter.ToUInt16(response, num);
                for (var i = 0; i < num3; i++)
                {
                    var num4 = BitConverter.ToUInt16(response, num + 2 + i * 2) + num;
                    var num5 = i == num3 - 1 ? response.Length : BitConverter.ToUInt16(response, num + 4 + i * 2) + num;
                    var num6 = BitConverter.ToUInt16(response, num4 + 2);
                    switch (num6)
                    {
                        case 4:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley04
                            };
                        case 5:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley05
                            };
                        case 6:
                            if (response[num + 2] == 210 || response[num + 2] == 204)
                            {
                                return new OperateResult<byte[], ushort, bool>
                                {
                                    ErrorCode = num6,
                                    Message = StringResources.Language.AllenBradley06
                                };
                            }
                            break;
                        case 10:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley0A
                            };
                        case 12:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley0C
                            };
                        case 19:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley13
                            };
                        case 28:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley1C
                            };
                        case 30:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley1E
                            };
                        case 38:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley26
                            };
                        default:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.UnknownError
                            };
                        case 0:
                            break;
                    }
                    if (isRead)
                    {
                        for (var j = num4 + 6; j < num5; j++)
                        {
                            list.Add(response[j]);
                        }
                    }
                }
            }
            else
            {
                var b = response[num + 4];
                switch (b)
                {
                    case 4:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley04
                        };
                    case 5:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley05
                        };
                    case 6:
                        value = true;
                        break;
                    case 10:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley0A
                        };
                    case 12:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley0C
                        };
                    case 19:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley13
                        };
                    case 28:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley1C
                        };
                    case 30:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley1E
                        };
                    case 32:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley20
                        };
                    case 38:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley26
                        };
                    default:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.UnknownError
                        };
                    case 0:
                        break;
                }
                if (response[num + 2] == 205 || response[num + 2] == 211)
                {
                    return OperateResult.CreateSuccessResult(list.ToArray(), value2, value);
                }
                if (response[num + 2] == 204 || response[num + 2] == 210)
                {
                    for (var k = num + 8; k < num + 2 + num2; k++)
                    {
                        list.Add(response[k]);
                    }
                    value2 = BitConverter.ToUInt16(response, num + 6);
                }
                else if (response[num + 2] == 213)
                {
                    for (var l = num + 6; l < num + 2 + num2; l++)
                    {
                        list.Add(response[l]);
                    }
                }
            }
            return OperateResult.CreateSuccessResult(list.ToArray(), value2, value);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[], ushort, bool>("ExtractActualData failed: " + ex.Message + Environment.NewLine + response.ToHexString(' '));
        }
    }

    internal static OperateResult<string> ExtractActualString(OperateResult<byte[], ushort, bool> read, IByteTransform byteTransform, Encoding encoding)
    {
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        try
        {
            if (read.Content2 == 218)
            {
                if (read.Content1.Length >= 1)
                {
                    if (read.Content1[0] == 0)
                    {
                        return OperateResult.CreateSuccessResult(string.Empty);
                    }
                    if (read.Content1[0] >= read.Content1.Length)
                    {
                        return OperateResult.CreateSuccessResult(encoding.GetString(read.Content1));
                    }
                    return OperateResult.CreateSuccessResult(encoding.GetString(read.Content1, 1, read.Content1[0]));
                }
                return OperateResult.CreateSuccessResult(encoding.GetString(read.Content1));
            }
            if (read.Content1.Length >= 6)
            {
                var num = byteTransform.TransInt32(read.Content1, 2);
                if (num == 0)
                {
                    return OperateResult.CreateSuccessResult(string.Empty);
                }
                return OperateResult.CreateSuccessResult(encoding.GetString(read.Content1, 6, num));
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(read.Content1));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message + " Source: " + read.Content1.ToHexString(' '));
        }
    }

    /// <summary>
    /// 从PLC里读取当前PLC的型号信息。
    /// </summary>
    /// <param name="plc">PLC对象</param>
    /// <returns>型号数据信息</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteDevice plc)
    {
        var buffer = "00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes();
        var read = await plc.ReadFromCoreServerAsync(buffer).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        if (read.Content.Length > 59 && read.Content.Length >= 59 + read.Content[58])
        {
            return OperateResult.CreateSuccessResult(Encoding.UTF8.GetString(read.Content, 59, read.Content[58]));
        }
        return new OperateResult<string>("Data is too short: " + read.Content.ToHexString(' '));
    }

    /// <summary>
    /// 读取指定地址的日期数据，最小日期为 1970年1月1日，当PLC的变量类型为 "Date" 和 "TimeAndDate" 时，都可以用本方法读取。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <returns>日期结果对象</returns>
    public static async Task<OperateResult<DateTime>> ReadDateAsync(IReadWriteCip plc, string address)
    {
        var read = await plc.ReadInt64Async(address).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(read);
        }
        var tick = read.Content / 100;
        return OperateResult.CreateSuccessResult(new DateTime(1970, 1, 1).AddTicks(tick));
    }

    /// <summary>
    /// 使用日期格式（Date）将指定的数据写入到指定的地址里，PLC的地址类型变量必须为 "Date"，否则写入失败。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <param name="date">时间信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteDateAsync(IReadWriteCip plc, string address, DateTime date)
    {
        var tick = (date.Date - new DateTime(1970, 1, 1)).Ticks * 100;
        return await plc.WriteTagAsync(address, 8, plc.ByteTransform.TransByte(tick)).ConfigureAwait(false);
    }

    /// <summary>
    /// 使用日期格式（Date）将指定的数据写入到指定的地址里，PLC的地址类型变量必须为 "Date"，否则写入失败。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <param name="date">时间信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteTimeAndDateAsync(IReadWriteCip plc, string address, DateTime date)
    {
        var tick = (date - new DateTime(1970, 1, 1)).Ticks * 100;
        return await plc.WriteTagAsync(address, 10, plc.ByteTransform.TransByte(tick)).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取指定地址的时间数据，最小时间为 0，如果获取秒，可以访问 <see cref="P:System.TimeSpan.TotalSeconds" />，当PLC的变量类型为 "Time" 和 "TimeOfDate" 时，都可以用本方法读取。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <returns>时间的结果对象</returns>
    public static async Task<OperateResult<TimeSpan>> ReadTimeAsync(IReadWriteCip plc, string address)
    {
        var read = await plc.ReadInt64Async(address).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TimeSpan>(read);
        }
        var tick = read.Content / 100;
        return OperateResult.CreateSuccessResult(TimeSpan.FromTicks(tick));
    }

    /// <summary>
    /// 使用时间格式（TIME）将时间数据写入到PLC中指定的地址里去，PLC的地址类型变量必须为 "TIME"，否则写入失败。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <param name="time">时间参数变量</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteTimeAsync(IReadWriteCip plc, string address, TimeSpan time)
    {
        return await plc.WriteTagAsync(address, 9, plc.ByteTransform.TransByte(time.Ticks * 100)).ConfigureAwait(false);
    }

    /// <summary>
    /// 使用时间格式（TimeOfDate）将时间数据写入到PLC中指定的地址里去，PLC的地址类型变量必须为 "TimeOfDate"，否则写入失败。
    /// </summary>
    /// <param name="plc">当前的通信对象信息</param>
    /// <param name="address">PLC里变量的地址</param>
    /// <param name="timeOfDate">时间参数变量</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteTimeOfDateAsync(IReadWriteCip plc, string address, TimeSpan timeOfDate)
    {
        return await plc.WriteTagAsync(address, 11, plc.ByteTransform.TransByte(timeOfDate.Ticks * 100)).ConfigureAwait(false);
    }
}
