using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Panasonic.Helper;

/// <summary>
/// Mewtocol协议的辅助类信息。
/// </summary>
public static class MewtocolHelper
{
    /// <summary>
    /// 读取单个的地址信息的bool值，地址举例：SR0.0 X0.0 Y0.0 R0.0 L0.0。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <returns>读取结果对象</returns>
    public static async Task<OperateResult<bool>> ReadBoolAsync(IReadWriteDevice plc, byte station, string address)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return ByteTransformHelper.GetResultFromArray(await CommunicationHelper.ReadBoolAsync(plc, address, 1).ConfigureAwait(continueOnCapturedContext: false));
        }
        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var command = PanasonicHelper.BuildReadOneCoil(station, address);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(command);
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(read);
        }
        return ByteTransformHelper.GetResultFromArray(PanasonicHelper.ExtraActualBool(read.Content));
    }

    /// <summary>
    /// 批量读取松下PLC的位数据，按照字为单位，地址为 X0,X10,Y10，读取的长度为16的倍数。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>读取结果对象</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return await CommunicationHelper.ReadBoolAsync(plc, address, length).ConfigureAwait(continueOnCapturedContext: false);
        }

        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var analysis = PanasonicHelper.AnalysisAddress(address);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }
        var command = PanasonicHelper.BuildReadCommand(station, address, length, isBit: true);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var list = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            var extra = PanasonicHelper.ExtraActualData(read.Content);
            if (!extra.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(extra);
            }
            list.AddRange(extra.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray().ToBoolArray().SelectMiddle(analysis.Content2 % 16, length));
    }

    /// <summary>
    /// 批量读取松下PLC的位数据，传入一个读取的地址列表，地址支持 X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">等待读取的地址列表，数组长度不限制</param>
    /// <returns>读取结果对象</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteDevice plc, byte station, string[] address)
    {
        var command = PanasonicHelper.BuildReadCoils(station, address);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var list = new List<bool>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            var extra = PanasonicHelper.ExtraActualBool(read.Content);
            if (!extra.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(extra);
            }
            list.AddRange(extra.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <summary>
    /// 往指定的地址写入bool数据，地址举例：SR0.0 X0.0 Y0.0 R0.0 L0.0。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="value">数据值信息</param>
    /// <returns>返回是否成功的结果对象</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, bool value)
    {
        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var command = PanasonicHelper.BuildWriteOneCoil(station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return PanasonicHelper.ExtraActualData(read.Content);
    }

    /// <summary>
    /// 往指定的地址写入 <see cref="bool" /> 数组，地址举例 X0.0 Y0.0 R0.0 L0.0，起始的位地址必须为16的倍数，写入的 <see cref="bool" /> 数组长度也为16的倍数。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="values">数据值信息</param>
    /// <returns>返回是否成功的结果对象</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, bool[] values)
    {
        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var analysis = PanasonicHelper.AnalysisAddress(address);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }
        if (analysis.Content2 % 16 != 0)
        {
            return new OperateResult(StringResources.Language.PanasonicAddressBitStartMulti16);
        }
        if (values.Length % 16 != 0)
        {
            return new OperateResult(StringResources.Language.PanasonicBoolLengthMulti16);
        }
        var command = PanasonicHelper.BuildWriteCommand(station, address, SoftBasic.BoolArrayToByte(values));
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return PanasonicHelper.ExtraActualData(read.Content);
    }

    /// <summary>
    /// 将Bool数组值写入到指定的离散地址里，一个地址对应一个bool值，地址数组长度和值数组长度必须相等，地址支持X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">离散的地址列表</param>
    /// <param name="value">bool数组值</param>
    /// <returns>是否写入成功的结果对象</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string[] address, bool[] value)
    {
        var command = PanasonicHelper.BuildWriteCoils(station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return read;
            }
            OperateResult extra = PanasonicHelper.ExtraActualData(read.Content);
            if (!extra.IsSuccess)
            {
                return extra;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 读取指定地址的原始数据，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="length">长度</param>
    /// <returns>原始的字节数据的信息</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var command = PanasonicHelper.BuildReadCommand(station, address, length, isBit: false);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var list = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await plc.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var extra = PanasonicHelper.ExtraActualData(read.Content);
            if (!extra.IsSuccess)
            {
                return extra;
            }
            list.AddRange(extra.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <summary>
    /// 将数据写入到指定的地址里去，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100。
    /// </summary>
    /// <param name="plc">PLC对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="value">真实数据</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, byte[] value)
    {
        station = (byte)CommunicationHelper.ExtractParameter(ref address, "s", station);
        var command = PanasonicHelper.BuildWriteCommand(station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await plc.ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return PanasonicHelper.ExtraActualData(read.Content);
    }

    private static OperateResult<string> GetPlcType(byte[] data)
    {
        try
        {
            var @string = Encoding.ASCII.GetString(data, 0, 2);
            return @string switch
            {
                "03" => OperateResult.CreateSuccessResult("FP3"),
                "02" => OperateResult.CreateSuccessResult("FP5"),
                "05" => OperateResult.CreateSuccessResult("FP-E"),
                _ => OperateResult.CreateSuccessResult(@string),
            };
        }
        catch (Exception ex)
        {
            return new OperateResult<string>("Get plctype failed : " + ex.Message + Environment.NewLine + "Source: " + data.ToHexString());
        }
    }

    /// <summary>
    /// 读取PLC的型号信息。
    /// </summary>
    /// <param name="plc">通信对象</param>
    /// <param name="station">站号信息</param>
    /// <returns>PLC型号</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteDevice plc, byte station)
    {
        var contents = PanasonicHelper.BuildReadPlcModel(station);
        var operateResult = await plc.ReadFromCoreServerAsync(contents).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        var operateResult2 = PanasonicHelper.ExtraActualData(operateResult.Content, parseData: false);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult2);
        }
        return GetPlcType(operateResult2.Content);
    }

    private static bool CheckBoolOnWordAddress(string address)
    {
        return Regex.IsMatch(address, "^(s=[0-9]+;)?(D|LD)[0-9]+\\.[0-9]+$", RegexOptions.IgnoreCase);
    }
}
