using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Profinet.Panasonic;

/// <summary>
/// 松下PLC的辅助类，提供了基本的辅助方法，用于解析地址，计算校验和，创建报文。
/// </summary>
public static class PanasonicHelper
{
   
    /// <summary>
    /// 位地址转换方法，101等同于10.1等同于10*16+1=161。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="fromBase">倍率信息</param>
    /// <returns>实际的位地址信息</returns>
    public static int CalculateComplexAddress(string address, int fromBase = 16)
    {
        if (address.IndexOf('.') < 0)
        {
            if (address.Length == 1)
            {
                return Convert.ToInt32(address, fromBase);
            }
            return Convert.ToInt32(address[..^1]) * fromBase + Convert.ToInt32(address[^1..], fromBase);
        }
        var num = Convert.ToInt32(address[..address.IndexOf('.')]) * fromBase;
        var bit = address[(address.IndexOf('.') + 1)..];
        return num + CommHelper.CalculateBitStartIndex(bit);
    }

    /// <summary>
    /// 解析数据地址，解析出地址类型，起始地址。
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <returns>解析出地址类型，起始地址</returns>
    public static OperateResult<string, int> AnalysisAddress(string address)
    {
        var operateResult = new OperateResult<string, int>();
        try
        {
            operateResult.Content2 = 0;
            if (address.StartsWith("IX") || address.StartsWith("ix"))
            {
                operateResult.Content1 = "IX";
                operateResult.Content2 = int.Parse(address[2..]);
            }
            else if (address.StartsWith("IY") || address.StartsWith("iy"))
            {
                operateResult.Content1 = "IY";
                operateResult.Content2 = int.Parse(address[2..]);
            }
            else if (address.StartsWith("ID") || address.StartsWith("id"))
            {
                operateResult.Content1 = "ID";
                operateResult.Content2 = int.Parse(address[2..]);
            }
            else if (address.StartsWith("SR") || address.StartsWith("sr"))
            {
                operateResult.Content1 = "SR";
                operateResult.Content2 = CalculateComplexAddress(address[2..]);
            }
            else if (address.StartsWith("LD") || address.StartsWith("ld"))
            {
                operateResult.Content1 = "LD";
                operateResult.Content2 = int.Parse(address[2..]);
            }
            else if (address[0] is 'X' or 'x')
            {
                operateResult.Content1 = "X";
                operateResult.Content2 = CalculateComplexAddress(address[1..]);
            }
            else if (address[0] is 'Y' or 'y')
            {
                operateResult.Content1 = "Y";
                operateResult.Content2 = CalculateComplexAddress(address[1..]);
            }
            else if (address[0] is 'R' or 'r')
            {
                operateResult.Content1 = "R";
                operateResult.Content2 = CalculateComplexAddress(address[1..]);
            }
            else if (address[0] is 'T' or 't')
            {
                operateResult.Content1 = "T";
                operateResult.Content2 = int.Parse(address[1..]);
            }
            else if (address[0] is 'C' or 'c')
            {
                operateResult.Content1 = "C";
                operateResult.Content2 = int.Parse(address[1..]);
            }
            else if (address[0] is 'L' or 'l')
            {
                operateResult.Content1 = "L";
                operateResult.Content2 = CalculateComplexAddress(address[1..]);
            }
            else if (address[0] is 'D' or 'd')
            {
                operateResult.Content1 = "D";
                operateResult.Content2 = int.Parse(address[1..]);
            }
            else if (address[0] is 'F' or 'f')
            {
                operateResult.Content1 = "F";
                operateResult.Content2 = int.Parse(address[1..]);
            }
            else if (address[0] is 'S' or 's')
            {
                operateResult.Content1 = "S";
                operateResult.Content2 = int.Parse(address[1..]);
            }
            else
            {
                if (address[0] is not 'K' and not 'k')
                {
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                }
                operateResult.Content1 = "K";
                operateResult.Content2 = int.Parse(address[1..]);
            }
        }
        catch (Exception ex)
        {
            operateResult.Message = ex.Message;
            return operateResult;
        }
        operateResult.IsSuccess = true;
        return operateResult;
    }

    /// <summary>
    /// 将松下的命令打包成带有%开头，CRC校验，CR结尾的完整的命令报文。如果参数 <c>useExpandedHeader</c> 设置为 <c>Ture</c>，则命令头使用 &lt; 开头
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="cmd">松下的命令。例如 RCSR100F</param>
    /// <param name="useExpandedHeader">设置是否使用扩展的命令头消息</param>
    /// <returns>原始的字节数组的命令</returns>
    private static byte[] PackPanasonicCommand(byte station, string cmd, bool useExpandedHeader)
    {
        var stringBuilder = new StringBuilder(useExpandedHeader ? "<" : "%");
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append(cmd);
        stringBuilder.Append(CalculateCrc(stringBuilder));
        stringBuilder.Append('\r');
        return Encoding.ASCII.GetBytes(stringBuilder.ToString());
    }

    private static OperateResult AppendCoil(StringBuilder sb, string address)
    {
        var operateResult = AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        sb.Append(operateResult.Content1);
        if (operateResult.Content1 is "X" or "Y" or "R" or "L")
        {
            sb.Append((operateResult.Content2 / 16).ToString("D3"));
            sb.Append((operateResult.Content2 % 16).ToString("X1"));
        }
        else
        {
            if (!(operateResult.Content1 == "T") && !(operateResult.Content1 == "C"))
            {
                return new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType);
            }
            sb.Append('0');
            sb.Append(operateResult.Content2.ToString("D3"));
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 创建读取离散触点的报文指令。
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="address">地址信息</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildReadOneCoil(byte station, string address)
    {
        if (address == null)
        {
            return new OperateResult<byte[]>("address is not allowed null");
        }
        if (address.Length < 1 || address.Length > 8)
        {
            return new OperateResult<byte[]>("length must be 1-8");
        }

        var stringBuilder = new StringBuilder("#RCS");
        var operateResult = AppendCoil(stringBuilder, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(PackPanasonicCommand(station, stringBuilder.ToString(), false));
    }

    /// <summary>
    /// 创建读取多个bool值得报文命令
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="address">等待读取的地址数组</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<List<byte[]>> BuildReadCoils(byte station, string[] address)
    {
        var list = new List<byte[]>();
        var list2 = CollectionUtils.SplitByLength(address, 8);
        for (var i = 0; i < list2.Count; i++)
        {
            var stringBuilder = new StringBuilder("#RCP");
            stringBuilder.Append(list2[i].Length);
            for (var j = 0; j < list2[i].Length; j++)
            {
                var operateResult = AppendCoil(stringBuilder, list2[i][j]);
                if (!operateResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
                }
            }
            list.Add(PackPanasonicCommand(station, stringBuilder.ToString(), false));
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 创建写入离散触点的报文指令。
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="address">地址信息</param>
    /// <param name="value">bool值数组</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildWriteOneCoil(byte station, string address, bool value)
    {
        var stringBuilder = new StringBuilder("#WCS");
        var operateResult = AppendCoil(stringBuilder, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        stringBuilder.Append(value ? '1' : '0');
        return OperateResult.CreateSuccessResult(PackPanasonicCommand(station, stringBuilder.ToString(), false));
    }

    /// <summary>
    /// 创建写入多个离散触点的报文指令
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="address">等待写入的地址列表</param>
    /// <param name="value">等待写入的值列表，长度应和地址长度一致</param>
    /// <returns>所有写入命令的报文列表</returns>
    public static OperateResult<List<byte[]>> BuildWriteCoils(byte station, string[] address, bool[] value)
    {
        if (address == null)
        {
            return new OperateResult<List<byte[]>>("Parameter address can't be null");
        }
        if (value == null)
        {
            return new OperateResult<List<byte[]>>("Parameter value can't be null");
        }
        if (address.Length != value.Length)
        {
            return new OperateResult<List<byte[]>>("Parameter address and parameter value, length is not same!");
        }

        var list = new List<byte[]>();
        var list2 = CollectionUtils.SplitByLength(address, 8);
        var list3 = CollectionUtils.SplitByLength(value, 8);
        for (var i = 0; i < list2.Count; i++)
        {
            var stringBuilder = new StringBuilder("#WCP");
            stringBuilder.Append(list2[i].Length);
            for (var j = 0; j < list2[i].Length; j++)
            {
                var operateResult = AppendCoil(stringBuilder, list2[i][j]);
                if (!operateResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
                }
                stringBuilder.Append(list3[i][j] ? '1' : '0');
            }
            list.Add(PackPanasonicCommand(station, stringBuilder.ToString(), false));
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 创建批量读取触点的报文指令。
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <param name="isBit">是否进行位为单位</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<List<byte[]>> BuildReadCommand(byte station, string address, ushort length, bool isBit)
    {
        if (address == null)
        {
            return new OperateResult<List<byte[]>>(StringResources.Language.PanasonicAddressParameterCannotBeNull);
        }
        var operateResult = AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var list = new List<byte[]>();
        if (isBit)
        {
            length += (ushort)(operateResult.Content2 % 16);
            operateResult.Content2 -= operateResult.Content2 % 16;
            var array = CollectionUtils.SplitIntegerToArray(length, 400);
            foreach (var num in array)
            {
                var stringBuilder = new StringBuilder("#");
                if (operateResult.Content1 is "X" or "Y" or "R" or "L")
                {
                    stringBuilder.Append("RCC");
                    stringBuilder.Append(operateResult.Content1);
                    var num2 = operateResult.Content2 / 16;
                    var num3 = (operateResult.Content2 + num - 1) / 16;
                    stringBuilder.Append(num2.ToString("D4"));
                    stringBuilder.Append(num3.ToString("D4"));
                    operateResult.Content2 += num;
                    list.Add(PackPanasonicCommand(station, stringBuilder.ToString(), false));
                    continue;
                }
                return new OperateResult<List<byte[]>>("Bit read only support X,Y,R,L");
            }
            return OperateResult.CreateSuccessResult(list);
        }

        var array2 = CollectionUtils.SplitIntegerToArray(length, 500);
        foreach (var num4 in array2)
        {
            var stringBuilder2 = new StringBuilder("#");
            if (operateResult.Content1 is "X" or "Y" or "R" or "L")
            {
                stringBuilder2.Append("RCC");
                stringBuilder2.Append(operateResult.Content1);
                var num5 = operateResult.Content2 / 16;
                var num6 = (operateResult.Content2 + (num4 - 1) * 16) / 16;
                stringBuilder2.Append(num5.ToString("D4"));
                stringBuilder2.Append(num6.ToString("D4"));
                operateResult.Content2 += num4 * 16;
            }
            else if (operateResult.Content1 is "D" or "LD" or "F")
            {
                stringBuilder2.Append("RD");
                stringBuilder2.Append(operateResult.Content1.AsSpan(0, 1));
                stringBuilder2.Append(operateResult.Content2.ToString("D5"));
                stringBuilder2.Append((operateResult.Content2 + num4 - 1).ToString("D5"));
                operateResult.Content2 += num4;
            }
            else if (operateResult.Content1 is "IX" or "IY" or "ID")
            {
                stringBuilder2.Append("RD");
                stringBuilder2.Append(operateResult.Content1);
                stringBuilder2.Append("000000000");
                operateResult.Content2 += num4;
            }
            else if (operateResult.Content1 is "C" or "T")
            {
                stringBuilder2.Append("RS");
                stringBuilder2.Append(operateResult.Content2.ToString("D4"));
                stringBuilder2.Append((operateResult.Content2 + num4 - 1).ToString("D4"));
                operateResult.Content2 += num4;
            }
            else
            {
                if (operateResult.Content1 is not "K" and not "S")
                {
                    return new OperateResult<List<byte[]>>(StringResources.Language.NotSupportedDataType);
                }
                stringBuilder2.Append('R');
                stringBuilder2.Append(operateResult.Content1);
                stringBuilder2.Append(operateResult.Content2.ToString("D4"));
                stringBuilder2.Append((operateResult.Content2 + num4 - 1).ToString("D4"));
                operateResult.Content2 += num4;
            }
            list.Add(PackPanasonicCommand(station, stringBuilder2.ToString(), num4 > 27));
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 创建批量读取触点的报文指令。
    /// </summary>
    /// <param name="station">设备站号</param>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据值</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildWriteCommand(byte station, string address, byte[] values)
    {
        if (address == null)
        {
            return new OperateResult<byte[]>(StringResources.Language.PanasonicAddressParameterCannotBeNull);
        }
        var operateResult = AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        values = CollectionUtils.ExpandToEvenLength(values);
        var num = (short)(values.Length / 2);
        var stringBuilder = new StringBuilder("#");
        if (operateResult.Content1 is "X" or "Y" or "R" or "L")
        {
            stringBuilder.Append("WCC");
            stringBuilder.Append(operateResult.Content1);
            var num2 = operateResult.Content2 / 16;
            var num3 = num2 + num - 1;
            stringBuilder.Append(num2.ToString("D4"));
            stringBuilder.Append(num3.ToString("D4"));
        }
        else if (operateResult.Content1 is "D" or "LD" or "F")
        {
            stringBuilder.Append("WD");
            stringBuilder.Append(operateResult.Content1.AsSpan(0, 1));
            stringBuilder.Append(operateResult.Content2.ToString("D5"));
            stringBuilder.Append((operateResult.Content2 + num - 1).ToString("D5"));
        }
        else if (operateResult.Content1 is "IX" or "IY" or "ID")
        {
            stringBuilder.Append("WD");
            stringBuilder.Append(operateResult.Content1);
            stringBuilder.Append(operateResult.Content2.ToString("D9"));
            stringBuilder.Append((operateResult.Content2 + num - 1).ToString("D9"));
        }
        else if (operateResult.Content1 is "C" or "T")
        {
            stringBuilder.Append("WS");
            stringBuilder.Append(operateResult.Content2.ToString("D4"));
            stringBuilder.Append((operateResult.Content2 + num - 1).ToString("D4"));
        }
        else if (operateResult.Content1 is "K" or "S")
        {
            stringBuilder.Append('W');
            stringBuilder.Append(operateResult.Content1);
            stringBuilder.Append(operateResult.Content2.ToString("D4"));
            stringBuilder.Append((operateResult.Content2 + num - 1).ToString("D4"));
        }
        stringBuilder.Append(values.ToHexString());
        return OperateResult.CreateSuccessResult(PackPanasonicCommand(station, stringBuilder.ToString(), stringBuilder.Length > 112));
    }

    /// <summary>
    /// 构建获取PLC型号的报文命令
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <returns>读取PLC型号的命令报文信息</returns>
    public static byte[] BuildReadPlcModel(byte station)
    {
        var stringBuilder = new StringBuilder("#");
        stringBuilder.Append("RT");
        return PackPanasonicCommand(station, stringBuilder.ToString(), stringBuilder.Length > 112);
    }

    /// <summary>
    /// 检查从PLC反馈的数据，并返回正确的数据内容。
    /// </summary>
    /// <param name="response">反馈信号</param>
    /// <param name="parseData">是否解析数据内容部分</param>
    /// <returns>是否成功的结果信息</returns>
    public static OperateResult<byte[]> ExtraActualData(byte[] response, bool parseData = true)
    {
        if (response.Length < 9)
        {
            return new OperateResult<byte[]>(StringResources.Language.PanasonicReceiveLengthMustLargerThan9);
        }

        try
        {
            if (response[3] == 36)
            {
                var array = new byte[response.Length - 9];
                if (array.Length != 0)
                {
                    Array.Copy(response, 6, array, 0, array.Length);
                    if (parseData)
                    {
                        array = Encoding.ASCII.GetString(array).ToHexBytes();
                    }
                }
                return OperateResult.CreateSuccessResult(array);
            }
            if (response[3] == 33)
            {
                var err = int.Parse(Encoding.ASCII.GetString(response, 4, 2));
                return new OperateResult<byte[]>(err, GetErrorDescription(err));
            }
            return new OperateResult<byte[]>(StringResources.Language.UnknownError + " Source Data: " + response.ToAsciiString());
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("ExtraActualData failed: " + ex.Message + Environment.NewLine + "Source: " + response.ToAsciiString());
        }
    }

    /// <summary>
    /// 检查从PLC反馈的数据，并返回正确的数据内容。
    /// </summary>
    /// <param name="response">反馈信号</param>
    /// <returns>是否成功的结果信息</returns>
    public static OperateResult<bool[]> ExtraActualBool(byte[] response)
    {
        if (response.Length < 9)
        {
            return new OperateResult<bool[]>(StringResources.Language.PanasonicReceiveLengthMustLargerThan9 + " Source: " + response.ToAsciiString());
        }
        if (response[3] == 36)
        {
            var source = response.SelectMiddle(6, response.Length - 9);
            return OperateResult.CreateSuccessResult(source.Select((m) => m == 49).ToArray());
        }
        if (response[3] == 33)
        {
            var err = int.Parse(Encoding.ASCII.GetString(response, 4, 2));
            return new OperateResult<bool[]>(err, GetErrorDescription(err));
        }
        return new OperateResult<bool[]>(StringResources.Language.UnknownError + " Source: " + response.ToAsciiString());
    }

    /// <summary>
    /// 根据错误码获取到错误描述文本。
    /// </summary>
    /// <param name="err">错误代码</param>
    /// <returns>字符信息</returns>
    public static string GetErrorDescription(int err)
    {
        return err switch
        {
            20 => StringResources.Language.PanasonicMewStatus20,
            21 => StringResources.Language.PanasonicMewStatus21,
            22 => StringResources.Language.PanasonicMewStatus22,
            23 => StringResources.Language.PanasonicMewStatus23,
            24 => StringResources.Language.PanasonicMewStatus24,
            25 => StringResources.Language.PanasonicMewStatus25,
            26 => StringResources.Language.PanasonicMewStatus26,
            27 => StringResources.Language.PanasonicMewStatus27,
            28 => StringResources.Language.PanasonicMewStatus28,
            29 => StringResources.Language.PanasonicMewStatus29,
            30 => StringResources.Language.PanasonicMewStatus30,
            40 => StringResources.Language.PanasonicMewStatus40,
            41 => StringResources.Language.PanasonicMewStatus41,
            42 => StringResources.Language.PanasonicMewStatus42,
            43 => StringResources.Language.PanasonicMewStatus43,
            50 => StringResources.Language.PanasonicMewStatus50,
            51 => StringResources.Language.PanasonicMewStatus51,
            52 => StringResources.Language.PanasonicMewStatus52,
            53 => StringResources.Language.PanasonicMewStatus53,
            60 => StringResources.Language.PanasonicMewStatus60,
            61 => StringResources.Language.PanasonicMewStatus61,
            62 => StringResources.Language.PanasonicMewStatus62,
            63 => StringResources.Language.PanasonicMewStatus63,
            64 => StringResources.Language.PanasonicMewStatus64,
            65 => StringResources.Language.PanasonicMewStatus65,
            66 => StringResources.Language.PanasonicMewStatus66,
            67 => StringResources.Language.PanasonicMewStatus67,
            68 => StringResources.Language.PanasonicMewStatus68,
            71 => StringResources.Language.PanasonicMewStatus71,
            78 => StringResources.Language.PanasonicMewStatus78,
            80 => StringResources.Language.PanasonicMewStatus80,
            81 => StringResources.Language.PanasonicMewStatus81,
            90 => StringResources.Language.PanasonicMewStatus90,
            92 => StringResources.Language.PanasonicMewStatus92,
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 根据MC的错误码去查找对象描述信息。
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>描述信息</returns>
    public static string GetMcErrorDescription(int code)
    {
        return code switch
        {
            16433 => StringResources.Language.PanasonicMc4031,
            49233 => StringResources.Language.PanasonicMcC051,
            49238 => StringResources.Language.PanasonicMcC056,
            49241 => StringResources.Language.PanasonicMcC059,
            49243 => StringResources.Language.PanasonicMcC05B,
            49244 => StringResources.Language.PanasonicMcC05C,
            49247 => StringResources.Language.PanasonicMcC05F,
            49248 => StringResources.Language.PanasonicMcC060,
            49249 => StringResources.Language.PanasonicMcC061,
            _ => StringResources.Language.MelsecPleaseReferToManualDocument,
        };
    }

    private static string CalculateCrc(StringBuilder sb)
    {
        var b = (byte)sb[0];
        for (var i = 1; i < sb.Length; i++)
        {
            b ^= (byte)sb[i];
        }
        return ByteExtensions.ToHexString([b]);
    }
}
