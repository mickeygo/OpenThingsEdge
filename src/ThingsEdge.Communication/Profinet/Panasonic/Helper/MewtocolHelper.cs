using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Panasonic.Helper;

/// <summary>
/// Mewtocol协议的辅助类信息
/// </summary>
public class MewtocolHelper
{
    private static bool CheckBoolOnWordAddress(string address)
    {
        return Regex.IsMatch(address, "^(s=[0-9]+;)?(D|LD)[0-9]+\\.[0-9]+$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 读取单个的地址信息的bool值，地址举例：SR0.0  X0.0  Y0.0  R0.0  L0.0<br />
    /// Read the bool value of a single address, for example: SR0.0 X0.0 Y0.0 R0.0 L0.0
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <returns>读取结果对象</returns>
    public static OperateResult<bool> ReadBool(IReadWriteDevice plc, byte station, string address)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return ByteTransformHelper.GetResultFromArray(CommHelper.ReadBool(plc, address, 1));
        }
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.BuildReadOneCoil(station, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(operateResult);
        }
        OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(operateResult2);
        }
        return ByteTransformHelper.GetResultFromArray(PanasonicHelper.ExtraActualBool(operateResult2.Content));
    }

    /// <summary>
    /// 批量读取松下PLC的位数据，按照字为单位，地址为 X0,X10,Y10，读取的长度为16的倍数<br />
    /// Read the bit data of Panasonic PLC in batches, the unit is word, the address is X0, X10, Y10, and the read length is a multiple of 16
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>读取结果对象</returns>
    public static OperateResult<bool[]> ReadBool(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return CommHelper.ReadBool(plc, address, length);
        }
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var operateResult2 = PanasonicHelper.BuildReadCommand(station, address, length, isBit: true);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        var list = new List<byte>();
        for (var i = 0; i < operateResult2.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult3 = plc.ReadFromCoreServer(operateResult2.Content[i]);
            if (!operateResult3.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult3);
            }
            var operateResult4 = PanasonicHelper.ExtraActualData(operateResult3.Content);
            if (!operateResult4.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult4);
            }
            list.AddRange(operateResult4.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray().ToBoolArray().SelectMiddle(operateResult.Content2 % 16, length));
    }

    /// <summary>
    /// 批量读取松下PLC的位数据，传入一个读取的地址列表，地址支持X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A<br />
    /// Batch read the bit data of Panasonic PLC, pass in a read address list, the address supports X, Y, R, T, C, L, for example: R1.0, X2.0, R3.A
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">等待读取的地址列表，数组长度不限制</param>
    /// <returns>读取结果对象</returns>
    public static OperateResult<bool[]> ReadBool(IReadWriteDevice plc, byte station, string[] address)
    {
        var operateResult = PanasonicHelper.BuildReadCoils(station, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var list = new List<bool>();
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content[i]);
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult2);
            }
            var operateResult3 = PanasonicHelper.ExtraActualBool(operateResult2.Content);
            if (!operateResult3.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult3);
            }
            list.AddRange(operateResult3.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <summary>
    /// 往指定的地址写入bool数据，地址举例：SR0.0  X0.0  Y0.0  R0.0  L0.0<br />
    /// Write bool data to the specified address. Example address: SR0.0 X0.0 Y0.0 R0.0 L0.0
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="value">数据值信息</param>
    /// <returns>返回是否成功的结果对象</returns>
    public static OperateResult Write(IReadWriteDevice plc, byte station, string address, bool value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.BuildWriteOneCoil(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return PanasonicHelper.ExtraActualData(operateResult2.Content);
    }

    /// <summary>
    /// 往指定的地址写入 <see cref="T:System.Boolean" /> 数组，地址举例 X0.0  Y0.0  R0.0  L0.0，
    /// 起始的位地址必须为16的倍数，写入的 <see cref="T:System.Boolean" /> 数组长度也为16的倍数。<br />
    /// Write the <see cref="T:System.Boolean" /> array to the specified address, address example: SR0.0 X0.0 Y0.0 R0.0 L0.0, 
    /// the starting bit address must be a multiple of 16. <see cref="T:System.Boolean" /> The length of the array is also a multiple of 16. <br />
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="values">数据值信息</param>
    /// <returns>返回是否成功的结果对象</returns>
    public static OperateResult Write(IReadWriteDevice plc, byte station, string address, bool[] values)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        if (operateResult.Content2 % 16 != 0)
        {
            return new OperateResult(StringResources.Language.PanasonicAddressBitStartMulti16);
        }
        if (values.Length % 16 != 0)
        {
            return new OperateResult(StringResources.Language.PanasonicBoolLengthMulti16);
        }
        var values2 = SoftBasic.BoolArrayToByte(values);
        var operateResult2 = PanasonicHelper.BuildWriteCommand(station, address, values2);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        OperateResult<byte[]> operateResult3 = plc.ReadFromCoreServer(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return operateResult3;
        }
        return PanasonicHelper.ExtraActualData(operateResult3.Content);
    }

    /// <summary>
    /// 将Bool数组值写入到指定的离散地址里，一个地址对应一个bool值，地址数组长度和值数组长度必须相等，地址支持X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A<br />
    /// Write the Bool array value to the specified discrete address, one address corresponds to one bool value, 
    /// the length of the address array and the length of the value array must be equal, the address supports X, Y, R, T, C, L, for example: R1.0, X2.0, R3.A
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">离散的地址列表</param>
    /// <param name="value">bool数组值</param>
    /// <returns>是否写入成功的结果对象</returns>
    public static OperateResult Write(IReadWriteDevice plc, byte station, string[] address, bool[] value)
    {
        var operateResult = PanasonicHelper.BuildWriteCoils(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content[i]);
            if (!operateResult2.IsSuccess)
            {
                return operateResult2;
            }
            OperateResult operateResult3 = PanasonicHelper.ExtraActualData(operateResult2.Content);
            if (!operateResult3.IsSuccess)
            {
                return operateResult3;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 读取指定地址的原始数据，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100<br />
    /// Read the original data of the specified address, address example: D0 F0 K0 T0 C0, the address supports carrying station number information, for example: s=2;D100
    /// </summary>
    /// <param name="plc">PLC通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="length">长度</param>
    /// <returns>原始的字节数据的信息</returns>
    public static OperateResult<byte[]> Read(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.BuildReadCommand(station, address, length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var list = new List<byte>();
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content[i]);
            if (!operateResult2.IsSuccess)
            {
                return operateResult2;
            }
            var operateResult3 = PanasonicHelper.ExtraActualData(operateResult2.Content);
            if (!operateResult3.IsSuccess)
            {
                return operateResult3;
            }
            list.AddRange(operateResult3.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <summary>
    /// 将数据写入到指定的地址里去，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100<br />
    /// Write data to the specified address, address example: D0 F0 K0 T0 C0, the address supports carrying station number information, for example: s=2;D100
    /// </summary>
    /// <param name="plc">PLC对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="address">起始地址</param>
    /// <param name="value">真实数据</param>
    /// <returns>是否写入成功</returns>
    public static OperateResult Write(IReadWriteDevice plc, byte station, string address, byte[] value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = PanasonicHelper.BuildWriteCommand(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return PanasonicHelper.ExtraActualData(operateResult2.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String)" />
    public static async Task<OperateResult<bool>> ReadBoolAsync(IReadWriteDevice plc, byte station, string address)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return ByteTransformHelper.GetResultFromArray(await CommHelper.ReadBoolAsync(plc, address, 1).ConfigureAwait(continueOnCapturedContext: false));
        }
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        if (CheckBoolOnWordAddress(address))
        {
            return await CommHelper.ReadBoolAsync(plc, address, length).ConfigureAwait(continueOnCapturedContext: false);
        }
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String[])" />
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean)" />
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, bool value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, bool[] values)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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
        var command = PanasonicHelper.BuildWriteCommand(values: SoftBasic.BoolArrayToByte(values), station: station, address: address);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String[],System.Boolean[])" />
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Read(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteDevice plc, byte station, string address, ushort length)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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

    /// <inheritdoc cref="M:HslCommunication.Profinet.Panasonic.Helper.MewtocolHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Byte[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice plc, byte station, string address, byte[] value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
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
    /// 读取PLC的型号信息<br />
    /// Read the model information of the PLC
    /// </summary>
    /// <param name="plc">通信对象</param>
    /// <param name="station">站号信息</param>
    /// <returns>PLC型号</returns>
    public static OperateResult<string> ReadPlcType(IReadWriteDevice plc, byte station)
    {
        var operateResult = PanasonicHelper.BuildReadPlcModel(station);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        OperateResult<byte[]> operateResult2 = plc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult2);
        }
        var operateResult3 = PanasonicHelper.ExtraActualData(operateResult2.Content, parseData: false);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult3);
        }
        return GetPlcType(operateResult3.Content);
    }
}
