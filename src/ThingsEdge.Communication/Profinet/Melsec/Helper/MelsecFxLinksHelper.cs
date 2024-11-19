using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Serial;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的 FxLinks 的辅助方法信息。
/// </summary>
public static class MelsecFxLinksHelper
{
    /// <summary>
    /// 将当前的报文进行打包，根据和校验的方式以及格式信息来实现打包操作。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="command">原始的命令数据</param>
    /// <returns>打包后的命令</returns>
    public static byte[] PackCommandWithHeader(IReadWriteFxLinks plc, byte[] command)
    {
        if (command.Length > 3 && command[0] == 5)
        {
            return command;
        }

        var array = command;
        if (plc.SumCheck)
        {
            array = new byte[command.Length + 2];
            command.CopyTo(array, 0);
            SoftLRC.CalculateAccAndFill(array, 0, 2);
        }
        if (plc.Format == 1)
        {
            return SoftBasic.SpliceArray(new byte[1] { 5 }, array);
        }
        if (plc.Format == 4)
        {
            return SoftBasic.SpliceArray(new byte[1] { 5 }, array, "\r\n"u8.ToArray());
        }
        return SoftBasic.SpliceArray(new byte[1] { 5 }, array);
    }

    /// <summary>
    /// 创建一条读取的指令信息，需要指定一些参数
    /// </summary>
    /// <param name="station">PLC的站号</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <param name="isBool">是否位读取</param>
    /// <param name="waitTime">等待时间</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<List<byte[]>> BuildReadCommand(byte station, string address, ushort length, bool isBool, byte waitTime = 0)
    {
        var operateResult = MelsecFxLinksAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var array = SoftBasic.SplitIntegerToArray(length, isBool ? 256 : 64);
        var list = new List<byte[]>();
        for (var i = 0; i < array.Length; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(station.ToString("X2"));
            stringBuilder.Append("FF");
            if (isBool)
            {
                stringBuilder.Append("BR");
            }
            else if (operateResult.Content.AddressStart >= 10000)
            {
                stringBuilder.Append("QR");
            }
            else
            {
                stringBuilder.Append("WR");
            }
            stringBuilder.Append(waitTime.ToString("X"));
            stringBuilder.Append(operateResult.Content.ToString());
            if (array[i] == 256)
            {
                stringBuilder.Append("00");
            }
            else
            {
                stringBuilder.Append(array[i].ToString("X2"));
            }
            list.Add(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
            operateResult.Content.AddressStart += array[i];
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 创建一条别入bool数据的指令信息，需要指定一些参数
    /// </summary>
    /// <param name="station">站号</param>
    /// <param name="address">地址</param>
    /// <param name="value">数组值</param>
    /// <param name="waitTime">等待时间</param>
    /// <returns>是否创建成功</returns>
    public static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool[] value, byte waitTime = 0)
    {
        var operateResult = MelsecFxLinksAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("FF");
        stringBuilder.Append("BW");
        stringBuilder.Append(waitTime.ToString("X"));
        stringBuilder.Append(operateResult.Content.ToString());
        stringBuilder.Append(value.Length.ToString("X2"));
        for (var i = 0; i < value.Length; i++)
        {
            stringBuilder.Append(value[i] ? "1" : "0");
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    

    /// <summary>
    /// 创建启动PLC的报文信息
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="waitTime">等待时间</param>
    /// <returns>命令报文的结果内容对象</returns>
    public static OperateResult<byte[]> BuildStart(byte station, byte waitTime = 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("FF");
        stringBuilder.Append("RR");
        stringBuilder.Append(waitTime.ToString("X"));
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 创建启动PLC的报文信息
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="waitTime">等待时间</param>
    /// <returns>命令报文的结果内容对象</returns>
    public static OperateResult<byte[]> BuildStop(byte station, byte waitTime = 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("FF");
        stringBuilder.Append("RS");
        stringBuilder.Append(waitTime.ToString("X"));
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 创建读取PLC类型的命令报文
    /// </summary>
    /// <param name="station">站号信息</param>
    /// <param name="waitTime">等待实际</param>
    /// <returns>命令报文的结果内容对象</returns>
    public static OperateResult<byte[]> BuildReadPlcType(byte station, byte waitTime = 0)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("FF");
        stringBuilder.Append("PC");
        stringBuilder.Append(waitTime.ToString("X"));
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 从编码中提取PLC的型号信息
    /// </summary>
    /// <param name="code">编码</param>
    /// <returns>PLC的型号信息</returns>
    public static OperateResult<string> GetPlcTypeFromCode(string code)
    {
        return code switch
        {
            "F2" => OperateResult.CreateSuccessResult("FX1S"),
            "8E" => OperateResult.CreateSuccessResult("FX0N"),
            "8D" => OperateResult.CreateSuccessResult("FX2/FX2C"),
            "9E" => OperateResult.CreateSuccessResult("FX1N/FX1NC"),
            "9D" => OperateResult.CreateSuccessResult("FX2N/FX2NC"),
            "F4" => OperateResult.CreateSuccessResult("FX3G"),
            "F3" => OperateResult.CreateSuccessResult("FX3U/FX3UC"),
            "98" => OperateResult.CreateSuccessResult("A0J2HCPU"),
            "A1" => OperateResult.CreateSuccessResult("A1CPU /A1NCPU"),
            "A2" => OperateResult.CreateSuccessResult("A2CPU/A2NCPU/A2SCPU"),
            "92" => OperateResult.CreateSuccessResult("A2ACPU"),
            "93" => OperateResult.CreateSuccessResult("A2ACPU-S1"),
            "9A" => OperateResult.CreateSuccessResult("A2CCPU"),
            "82" => OperateResult.CreateSuccessResult("A2USCPU"),
            "83" => OperateResult.CreateSuccessResult("A2CPU-S1/A2USCPU-S1"),
            "A3" => OperateResult.CreateSuccessResult("A3CPU/A3NCPU"),
            "94" => OperateResult.CreateSuccessResult("A3ACPU"),
            "A4" => OperateResult.CreateSuccessResult("A3HCPU/A3MCPU"),
            "84" => OperateResult.CreateSuccessResult("A3UCPU"),
            "85" => OperateResult.CreateSuccessResult("A4UCPU"),
            "AB" => OperateResult.CreateSuccessResult("AJ72P25/R25"),
            "8B" => OperateResult.CreateSuccessResult("AJ72LP25/BR15"),
            _ => new OperateResult<string>(StringResources.Language.NotSupportedDataType + " Code:" + code),
        };
    }

    private static string GetErrorText(int error)
    {
        return error switch
        {
            2 => StringResources.Language.MelsecFxLinksError02,
            3 => StringResources.Language.MelsecFxLinksError03,
            6 => StringResources.Language.MelsecFxLinksError06,
            7 => StringResources.Language.MelsecFxLinksError07,
            10 => StringResources.Language.MelsecFxLinksError0A,
            16 => StringResources.Language.MelsecFxLinksError10,
            24 => StringResources.Language.MelsecFxLinksError18,
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 检查PLC的消息反馈是否合法，合法则提取当前的数据信息，当时写入的命令消息时，无任何的数据返回。
    /// </summary>
    /// <param name="response">从PLC反馈的数据消息</param>
    /// <returns>检查的结果消息</returns>
    public static OperateResult<byte[]> CheckPlcResponse(byte[] response)
    {
        try
        {
            if (response[0] == 21)
            {
                var num = Convert.ToInt32(Encoding.ASCII.GetString(response, 5, 2), 16);
                return new OperateResult<byte[]>(num, GetErrorText(num));
            }
            if (response[0] != 2 && response[0] != 6)
            {
                return new OperateResult<byte[]>(response[0], "Check command failed: " + SoftBasic.GetAsciiStringRender(response));
            }
            if (response[0] == 6)
            {
                return OperateResult.CreateSuccessResult(Array.Empty<byte>());
            }
            var num2 = -1;
            for (var i = 5; i < response.Length; i++)
            {
                if (response[i] == 3)
                {
                    num2 = i;
                    break;
                }
            }
            if (num2 == -1)
            {
                num2 = response.Length;
            }
            return OperateResult.CreateSuccessResult(response.SelectMiddle(5, num2 - 5));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Check Plc Response failed Error: " + ex.Message + " Source: " + SoftBasic.GetAsciiStringRender(response));
        }
    }

    /// <summary>
    /// 批量读取PLC的数据，以字为单位，支持读取 X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认，地址支持动态指定站号，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>读取结果信息</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteFxLinks plc, string address, ushort length)
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var command = BuildReadCommand(stat, address, length, isBool: false, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }

        var result = new List<byte>();
        for (var j = 0; j < command.Content.Count; j++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[j]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            var extra = CheckPlcResponse(read.Content);
            if (!extra.IsSuccess)
            {
                return extra;
            }
            var content = ExtraResponse(extra.Content);
            if (!content.IsSuccess)
            {
                return content;
            }
            result.AddRange(content.Content);
        }
        return OperateResult.CreateSuccessResult(result.ToArray());
    }

    /// <summary>
    /// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持 X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认，地址支持动态指定站号，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据值</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteFxLinks plc, string address, byte[] values)
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var command = BuildWriteByteCommand(stat, address, values, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = CheckPlcResponse(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型，地址支持动态指定站号，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
    /// <param name="length">读取的长度</param>
    /// <returns>读取结果信息</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteFxLinks plc, string address, ushort length)
    {
        if (address.IndexOf('.') > 0)
        {
            return await CommunicationHelper.ReadBoolAsync(plc, address, length).ConfigureAwait(false);
        }

        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var command = BuildReadCommand(stat, address, length, isBool: true, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var result = new List<bool>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            var extra = CheckPlcResponse(read.Content);
            if (!extra.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(extra);
            }
            result.AddRange(extra.Content.Select((m) => m == 49).ToArray());
        }
        return OperateResult.CreateSuccessResult(result.ToArray());
    }

    /// <summary>
    /// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认，地址支持动态指定站号，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据值</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteFxLinks plc, string address, bool[] values)
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var command = BuildWriteBoolCommand(stat, address, values, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = CheckPlcResponse(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 启动PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>是否启动成功</returns>
    public static async Task<OperateResult> StartPLCAsync(IReadWriteFxLinks plc, string parameter = "")
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref parameter, "s", plc.Station);
        var command = BuildStart(stat, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return command;
        }

        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = CheckPlcResponse(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 停止PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>是否停止成功</returns>
    public static async Task<OperateResult> StopPLCAsync(IReadWriteFxLinks plc, string parameter = "")
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref parameter, "s", plc.Station);
        var command = BuildStop(stat, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = CheckPlcResponse(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 读取PLC的型号信息，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
    /// <returns>带PLC型号的结果信息</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteFxLinks plc, string parameter = "")
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref parameter, "s", plc.Station);
        var command = BuildReadPlcType(stat, plc.WaittingTime);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(command);
        }

        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        var extra = CheckPlcResponse(read.Content);
        if (!extra.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(extra);
        }
        return GetPlcTypeFromCode(Encoding.ASCII.GetString(read.Content, 5, 2));
    }

    private static OperateResult<byte[]> ExtraResponse(byte[] response)
    {
        try
        {
            var array = new byte[response.Length / 2];
            for (var i = 0; i < array.Length / 2; i++)
            {
                var value = Convert.ToUInt16(Encoding.ASCII.GetString(response, i * 4, 4), 16);
                BitConverter.GetBytes(value).CopyTo(array, i * 2);
            }
            return OperateResult.CreateSuccessResult(array);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Extra source data failed: " + ex.Message + Environment.NewLine + "Source: " + response.ToHexString(' '));
        }
    }

    /// <summary>
    /// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位。
    /// </summary>
    /// <param name="station">站号</param>
    /// <param name="address">地址</param>
    /// <param name="value">数组值</param>
    /// <param name="waitTime">等待时间</param>
    /// <returns>命令报文的结果内容对象</returns>
    private static OperateResult<byte[]> BuildWriteByteCommand(byte station, string address, byte[] value, byte waitTime = 0)
    {
        var operateResult = MelsecFxLinksAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("FF");
        if (operateResult.Content.AddressStart >= 10000)
        {
            stringBuilder.Append("QW");
        }
        else
        {
            stringBuilder.Append("WW");
        }
        stringBuilder.Append(waitTime.ToString("X"));
        stringBuilder.Append(operateResult.Content.ToString());
        stringBuilder.Append((value.Length / 2).ToString("X2"));
        var array = new byte[value.Length * 2];
        for (var i = 0; i < value.Length / 2; i++)
        {
            SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(array, 4 * i);
        }
        stringBuilder.Append(Encoding.ASCII.GetString(array));
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }
}
