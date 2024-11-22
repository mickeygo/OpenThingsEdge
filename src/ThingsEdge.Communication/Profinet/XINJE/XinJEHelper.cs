using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.XINJE;

/// <summary>
/// 信捷PLC的相关辅助类
/// </summary>
public static class XinJEHelper
{
    /// <summary>
    /// 根据信捷PLC的地址，解析出转换后的modbus协议信息
    /// </summary>
    /// <param name="series">PLC的系列信息</param>
    /// <param name="address">汇川plc的地址信息</param>
    /// <param name="modbusCode">原始的对应的modbus信息</param>
    /// <returns>还原后的modbus地址</returns>
    public static OperateResult<string> PraseXinJEAddress(XinJESeries series, string address, byte modbusCode)
    {
        var text = string.Empty;
        var operateResult = CommHelper.ExtractParameter(ref address, "s");
        if (operateResult.IsSuccess)
        {
            text = $"s={operateResult.Content};";
        }
        var text2 = string.Empty;
        var operateResult2 = CommHelper.ExtractParameter(ref address, "x");
        if (operateResult2.IsSuccess)
        {
            text2 = $"x={operateResult2.Content};";
        }
        if (series == XinJESeries.XC)
        {
            try
            {
                // 以 X、Y、M 和 S 中的任何一个字符开头，"^(X|Y|M|S)[0-9]+" 等价于 "^[XY][0-9]+"。
                if (Regex.IsMatch(address, "^(X|Y|M|S)[0-9]+", RegexOptions.IgnoreCase) && modbusCode == 3)
                {
                    modbusCode = 1;
                    text2 = "x=1;";
                }
            }
            catch
            {
            }
            return PraseXinJEXCAddress(text + text2, address, modbusCode);
        }

        try
        {
            // 以 X、Y、M、S、SME、HSC、SM、ET、HM、HS、HT、HC 中的任何一个开头
            if (Regex.IsMatch(address, "^(X|Y|M|S|SME|HSC|SM|ET|HM|HS|HT|HC)[0-9]+", RegexOptions.IgnoreCase) && modbusCode == 3)
            {
                modbusCode = 1;
                text2 = "x=1;";
            }
        }
        catch
        {
        }
        return PraseXinJEXD1XD2XD3XL1XL3Address(text + text2, address, modbusCode);
    }

    private static int CalculateXC_D(string address)
    {
        var num = Convert.ToInt32(address);
        if (num >= 8000)
        {
            return num - 8000 + 16384;
        }
        return num;
    }

    /// <summary>
    /// 根据信捷PLC的地址，解析出转换后的modbus协议信息，适用XC系列
    /// </summary>
    /// <param name="station">站号的特殊指定信息，可以为空</param>
    /// <param name="address">信捷plc的地址信息</param>
    /// <param name="modbusCode">原始的对应的modbus信息</param>
    /// <returns>还原后的modbus地址</returns>
    public static OperateResult<string> PraseXinJEXCAddress(string station, string address, byte modbusCode)
    {
        try
        {
            if (modbusCode == 1 || modbusCode == 2 || modbusCode == 15 || modbusCode == 5)
            {
                if (ModbusHelper.TransAddressToModbus(station, address, ["X", "Y"], [16384, 18432], CalculateXinJEStartAddress, out var newAddress))
                {
                    return OperateResult.CreateSuccessResult(newAddress);
                }
                if (ModbusHelper.TransAddressToModbus(station, address, ["S", "T", "C"], [20480, 25600, 27648], out var newAddress2))
                {
                    return OperateResult.CreateSuccessResult(newAddress2);
                }
                if (address.StartsWithAndNextIsNumber("M"))
                {
                    var num = Convert.ToInt32(address[1..]);
                    if (num >= 8000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num - 8000 + 24576));
                    }
                    return OperateResult.CreateSuccessResult(station + num);
                }
                if (ModbusHelper.TransPointAddressToModbus(station, address, ["D"], new int[1], CalculateXC_D, out var newAddress3))
                {
                    return OperateResult.CreateSuccessResult(newAddress3);
                }
            }
            else
            {
                if (address.StartsWithAndNextIsNumber("D"))
                {
                    return OperateResult.CreateSuccessResult(station + CalculateXC_D(address.Substring(1)));
                }
                if (address.StartsWithAndNextIsNumber("F"))
                {
                    var num2 = Convert.ToInt32(address.Substring(1));
                    if (num2 >= 8000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num2 - 8000 + 26624));
                    }
                    return OperateResult.CreateSuccessResult(station + (num2 + 18432));
                }
                if (ModbusHelper.TransAddressToModbus(station, address, ["E", "T", "C"], [28672, 12288, 14336], out var newAddress4))
                {
                    return OperateResult.CreateSuccessResult(newAddress4);
                }
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }

    /// <summary>
    /// 解析信捷的XD1,XD2,XD3,XL1,XL3系列的PLC的Modbus地址和内部软元件的对照
    /// </summary>
    /// <remarks>适用 XD1、XD2、XD3、XL1、XL3、XD5、XDM、XDC、XD5E、XDME、XL5、XL5E、XLME, XDH 只是支持的地址范围不一样而已</remarks>
    /// <param name="station">站号的特殊指定信息，可以为空</param>
    /// <param name="address">PLC内部的软元件的地址</param>
    /// <param name="modbusCode">默认的Modbus功能码</param>
    /// <returns>解析后的Modbus地址</returns>
    public static OperateResult<string> PraseXinJEXD1XD2XD3XL1XL3Address(string station, string address, byte modbusCode)
    {
        try
        {
            if (modbusCode == 1 || modbusCode == 2 || modbusCode == 15 || modbusCode == 5)
            {
                if (address.StartsWith('X') || address.StartsWith('x'))
                {
                    var num = CalculateXinJEStartAddress(address[1..]);
                    if (num < 4096)
                    {
                        return OperateResult.CreateSuccessResult(station + (num + 20480));
                    }
                    if (num < 8192)
                    {
                        return OperateResult.CreateSuccessResult(station + (num - 4096 + 20736));
                    }
                    if (num < 12288)
                    {
                        return OperateResult.CreateSuccessResult(station + (num - 8192 + 22736));
                    }
                    return OperateResult.CreateSuccessResult(station + (num - 12288 + 23536));
                }
                if (address.StartsWith('Y') || address.StartsWith('y'))
                {
                    var num2 = CalculateXinJEStartAddress(address[1..]);
                    if (num2 < 4096)
                    {
                        return OperateResult.CreateSuccessResult(station + (num2 + 24576));
                    }
                    if (num2 < 8192)
                    {
                        return OperateResult.CreateSuccessResult(station + (num2 - 4096 + 24832));
                    }
                    if (num2 < 12288)
                    {
                        return OperateResult.CreateSuccessResult(station + (num2 - 8192 + 26832));
                    }
                    return OperateResult.CreateSuccessResult(station + (num2 - 12288 + 27632));
                }
                if (ModbusHelper.TransAddressToModbus(station, address,
                [
                    "SEM", "HSC", "SM", "ET", "HM", "HS", "HT", "HC", "S", "T",
                    "C", "M"
                ],
                [
                    49280, 59648, 36864, 49152, 49408, 55552, 57600, 58624, 28672, 40960,
                    45056, 0
                ], out var newAddress))
                {
                    return OperateResult.CreateSuccessResult(newAddress);
                }
                if (ModbusHelper.TransPointAddressToModbus(station, address, ["D", "SD", "HD"], [0, 28672, 41088], out var newAddress2))
                {
                    return OperateResult.CreateSuccessResult(newAddress2);
                }
            }
            else
            {
                if (address.StartsWith("ID") || address.StartsWith("id"))
                {
                    var num3 = Convert.ToInt32(address[2..]);
                    if (num3 < 10000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num3 + 20480));
                    }
                    if (num3 < 20000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num3 - 10000 + 20736));
                    }
                    if (num3 < 30000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num3 - 20000 + 22736));
                    }
                    return OperateResult.CreateSuccessResult(station + (num3 - 30000 + 23536));
                }
                if (address.StartsWith("QD") || address.StartsWith("qd"))
                {
                    var num4 = Convert.ToInt32(address[2..]);
                    if (num4 < 10000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num4 + 24576));
                    }
                    if (num4 < 20000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num4 - 10000 + 24832));
                    }
                    if (num4 < 30000)
                    {
                        return OperateResult.CreateSuccessResult(station + (num4 - 20000 + 26832));
                    }
                    return OperateResult.CreateSuccessResult(station + (num4 - 30000 + 27632));
                }
                if (ModbusHelper.TransAddressToModbus(station, address,
                [
                    "HSCD", "ETD", "HSD", "HTD", "HCD", "SFD", "SD", "TD", "CD", "HD",
                    "FD", "FS", "D"
                ],
                [
                    50304, 40960, 47232, 48256, 49280, 58560, 28672, 32768, 36864, 41088,
                    50368, 62656, 0
                ], out var newAddress3))
                {
                    return OperateResult.CreateSuccessResult(newAddress3);
                }
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }

    internal static OperateResult<List<byte[]>> BuildReadCommand(byte station, string address, ushort length, bool isBit)
    {
        var operateResult = XinJEAddress.ParseFrom(address, station);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }
        return BuildReadCommand(operateResult.Content, length, isBit);
    }

    internal static OperateResult<List<byte[]>> BuildReadCommand(XinJEAddress address, ushort length, bool isBit)
    {
        var list = new List<byte[]>();
        var array = CollectionUtils.SplitIntegerToArray(length, isBit ? 1920 : 120);
        for (var i = 0; i < array.Length; i++)
        {
            var item = new byte[8]
            {
                address.Station,
                (byte)(isBit ? 30 : 32),
                address.DataCode,
                BitConverter.GetBytes(address.AddressStart)[2],
                BitConverter.GetBytes(address.AddressStart)[1],
                BitConverter.GetBytes(address.AddressStart)[0],
                BitConverter.GetBytes(array[i])[1],
                BitConverter.GetBytes(array[i])[0]
            };
            address.AddressStart += array[i];
            list.Add(item);
        }
        return OperateResult.CreateSuccessResult(list);
    }

    internal static OperateResult<byte[]> BuildWriteWordCommand(byte station, string address, byte[] value)
    {
        var operateResult = XinJEAddress.ParseFrom(address, station);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return BuildWriteWordCommand(operateResult.Content, value);
    }

    internal static OperateResult<byte[]> BuildWriteWordCommand(XinJEAddress address, byte[] value)
    {
        var array = new byte[9 + value.Length];
        array[0] = address.Station;
        array[1] = 33;
        array[2] = address.DataCode;
        array[3] = BitConverter.GetBytes(address.AddressStart)[2];
        array[4] = BitConverter.GetBytes(address.AddressStart)[1];
        array[5] = BitConverter.GetBytes(address.AddressStart)[0];
        array[6] = BitConverter.GetBytes(value.Length / 2)[1];
        array[7] = BitConverter.GetBytes(value.Length / 2)[0];
        array[8] = (byte)value.Length;
        value.CopyTo(array, 9);
        return OperateResult.CreateSuccessResult(array);
    }

    internal static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool[] value)
    {
        var operateResult = XinJEAddress.ParseFrom(address, station);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(BuildWriteBoolCommand(operateResult.Content, value));
    }

    internal static async Task<OperateResult<byte[]>> ReadAsync(IModbus modbus, string address, ushort length, Func<string, ushort, Task<OperateResult<byte[]>>> funcRead)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcRead(address, length).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart + length <= xinJE.CriticalAddress)
        {
            return await funcRead(address, length).ConfigureAwait(false);
        }
        var result = new List<byte>();
        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            var read = await funcRead(address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            result.AddRange(read.Content);
            length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
            xinJE.AddressStart = xinJE.CriticalAddress;
        }
        var command = BuildReadCommand(analysis.Content, length, isBit: false);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var readAgain = await modbus.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!readAgain.IsSuccess)
        {
            return readAgain;
        }
        result.AddRange(readAgain.Content);
        return OperateResult.CreateSuccessResult(result.ToArray());
    }

    internal static async Task<OperateResult> WriteAsync(IModbus modbus, string address, byte[] value, Func<string, byte[], Task<OperateResult>> funcWrite)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart + value.Length / 2 <= xinJE.CriticalAddress)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }

        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            var write = await funcWrite(address, value.SelectBegin((xinJE.CriticalAddress - xinJE.AddressStart) * 2)).ConfigureAwait(false);
            if (!write.IsSuccess)
            {
                return write;
            }
            value = value.RemoveBegin((xinJE.CriticalAddress - xinJE.AddressStart) * 2);
            xinJE.AddressStart = xinJE.CriticalAddress;
        }
        var command = BuildWriteWordCommand(xinJE, value);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        return await modbus.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
    }

    internal static async Task<OperateResult> WriteAsync(IModbus modbus, string address, short value, Func<string, short, Task<OperateResult>> funcWrite)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        return await modbus.WriteAsync(address, modbus.ByteTransform.TransByte(value)).ConfigureAwait(false);
    }

    internal static async Task<OperateResult> WriteAsync(IModbus modbus, string address, ushort value, Func<string, ushort, Task<OperateResult>> funcWrite)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        return await modbus.WriteAsync(address, modbus.ByteTransform.TransByte(value)).ConfigureAwait(false);
    }

    internal static async Task<OperateResult<bool[]>> ReadBoolAsync(IModbus modbus, string address, ushort length, Func<string, ushort, Task<OperateResult<bool[]>>> funcRead)
    {
        var analysis = XinJEAddress.ParseFrom(address, length, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcRead(address, length).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart + length <= xinJE.CriticalAddress)
        {
            return await funcRead(address, length).ConfigureAwait(false);
        }

        var result = new List<bool>();
        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            var read = await funcRead(address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            result.AddRange(read.Content);
            length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
            xinJE.AddressStart = xinJE.CriticalAddress;
        }
        var command = BuildReadCommand(xinJE, length, isBit: true);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var readAgain = await modbus.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!readAgain.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(readAgain);
        }
        result.AddRange(readAgain.Content.ToBoolArray().SelectBegin(length));
        return OperateResult.CreateSuccessResult(result.ToArray());
    }

    internal static async Task<OperateResult> WriteAsync(IModbus modbus, string address, bool[] values, Func<string, bool[], Task<OperateResult>> funcWrite)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcWrite(address, values).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart + values.Length <= xinJE.CriticalAddress)
        {
            return await funcWrite(address, values).ConfigureAwait(false);
        }

        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            var write = await funcWrite(address, values.SelectBegin(xinJE.CriticalAddress - xinJE.AddressStart)).ConfigureAwait(false);
            if (!write.IsSuccess)
            {
                return write;
            }
            values = values.RemoveBegin(xinJE.CriticalAddress - xinJE.AddressStart);
            xinJE.AddressStart = xinJE.CriticalAddress;
        }
        var command = BuildWriteBoolCommand(xinJE, values);
        return await modbus.ReadFromCoreServerAsync(command).ConfigureAwait(false);
    }

    internal static async Task<OperateResult> WriteAsync(IModbus modbus, string address, bool value, Func<string, bool, Task<OperateResult>> funcWrite)
    {
        var analysis = XinJEAddress.ParseFrom(address, modbus.Station);
        if (!analysis.IsSuccess)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        var xinJE = analysis.Content;
        if (xinJE.AddressStart < xinJE.CriticalAddress)
        {
            return await funcWrite(address, value).ConfigureAwait(false);
        }
        return await modbus.WriteAsync(address, [value]).ConfigureAwait(false);
    }

    private static byte[] BuildWriteBoolCommand(XinJEAddress address, bool[] value)
    {
        var array = value.ToByteArray();
        var array2 = new byte[9 + array.Length];
        array2[0] = address.Station;
        array2[1] = 31;
        array2[2] = address.DataCode;
        array2[3] = BitConverter.GetBytes(address.AddressStart)[2];
        array2[4] = BitConverter.GetBytes(address.AddressStart)[1];
        array2[5] = BitConverter.GetBytes(address.AddressStart)[0];
        array2[6] = BitConverter.GetBytes(value.Length)[1];
        array2[7] = BitConverter.GetBytes(value.Length)[0];
        array2[8] = (byte)array.Length;
        array.CopyTo(array2, 9);
        return array2;
    }

    private static int CalculateXinJEStartAddress(string address)
    {
        if (address.IndexOf('.') < 0)
        {
            return Convert.ToInt32(address, 8);
        }
        var array = address.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
        return Convert.ToInt32(array[0], 8) * 8 + int.Parse(array[1]);
    }
}
