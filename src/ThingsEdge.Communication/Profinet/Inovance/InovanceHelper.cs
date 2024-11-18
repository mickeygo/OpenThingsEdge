using System;
using System.Text;
using System.Threading.Tasks;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.Inovance;

/// <summary>
/// 汇川PLC的辅助类，提供一些地址解析的方法<br />
/// Auxiliary class of Yaskawa robot, providing some methods of address resolution
/// </summary>
public class InovanceHelper
{
    private static int CalculateStartAddress(string address)
    {
        if (address.IndexOf('.') < 0)
        {
            return int.Parse(address);
        }
        var array = address.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        return int.Parse(array[0]) * 8 + int.Parse(array[1]);
    }

    /// <summary>
    /// 按照字节读取汇川M地址的数据，地址示例： MB100，MB101，需要注意的是，MB100 及 MB101 的地址是 MW50 的数据。<br />
    /// Read the data of Inovance M address according to the byte, address example: MB100, MB101, it should be noted that the addresses of MB100 and MB101 are the data of MW50.
    /// </summary>
    /// <param name="modbus">汇川的PLC对象</param>
    /// <param name="address">地址信息</param>
    /// <returns>读取的结果数据</returns>
    public static OperateResult<byte> ReadByte(IModbus modbus, string address)
    {
        try
        {
            var num = 0;
            if (address.StartsWith("MB") || address.StartsWith("mb"))
            {
                num = Convert.ToInt32(address.Substring(2));
            }
            else if (address.StartsWith("M") || address.StartsWith("m"))
            {
                num = Convert.ToInt32(address.Substring(1));
            }
            else
            {
                new OperateResult<string>(StringResources.Language.NotSupportedDataType);
            }
            OperateResult<byte[]> operateResult = modbus.Read("MW" + num / 2, 1);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte>(operateResult);
            }
            return OperateResult.CreateSuccessResult(num % 2 == 0 ? operateResult.Content[1] : operateResult.Content[0]);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte>("Address prase failed: " + ex.Message);
        }
    }

    internal static OperateResult<string> ReadAMString(IModbus modbus, string address, ushort length, Encoding encoding)
    {
        var num = Convert.ToInt32(address.Substring(address.Length - 1));
        address = address.Substring(0, address.Length - 1) + (num - 1);
        OperateResult<byte[]> operateResult = modbus.Read(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        if (modbus.IsStringReverse)
        {
            operateResult.Content = SoftBasic.BytesReverseByWord(operateResult.Content);
        }
        operateResult.Content = operateResult.Content.RemoveBegin(1);
        return OperateResult.CreateSuccessResult(encoding.GetString(operateResult.Content));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.ReadByte(HslCommunication.ModBus.IModbus,System.String)" />
    public static async Task<OperateResult<byte>> ReadByteAsync(IModbus modbus, string address)
    {
        try
        {
            var offset = 0;
            if (address.StartsWith("MB", StringComparison.OrdinalIgnoreCase))
            {
                offset = Convert.ToInt32(address.Substring(2));
            }
            else if (address.StartsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                offset = Convert.ToInt32(address.Substring(1));
            }
            else
            {
                new OperateResult<string>(StringResources.Language.NotSupportedDataType);
            }
            var read = await modbus.ReadAsync("MW" + offset / 2, 1);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte>(read);
            }
            return OperateResult.CreateSuccessResult(offset % 2 == 0 ? read.Content[1] : read.Content[0]);
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte>("Address prase failed: " + ex.Message);
        }
    }

    internal static async Task<OperateResult<string>> ReadAMStringAsync(IModbus modbus, string address, ushort length, Encoding encoding)
    {
        address = string.Concat(str1: (Convert.ToInt32(address.Substring(address.Length - 1)) - 1).ToString(), str0: address.Substring(0, address.Length - 1));
        var read = await modbus.ReadAsync(address, length);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        if (modbus.IsStringReverse)
        {
            read.Content = SoftBasic.BytesReverseByWord(read.Content);
        }
        read.Content = read.Content.RemoveBegin(1);
        return OperateResult.CreateSuccessResult(encoding.GetString(read.Content));
    }

    /// <summary>
    /// 根据汇川PLC的地址，解析出转换后的modbus协议信息，适用AM,H3U,H5U系列的PLC<br />
    /// According to the address of Inovance PLC, analyze the converted modbus protocol information, which is suitable for AM, H3U, H5U series PLC
    /// </summary>
    /// <param name="series">PLC的系列</param>
    /// <param name="address">汇川plc的地址信息</param>
    /// <param name="modbusCode">原始的对应的modbus信息</param>
    /// <returns>Modbus格式的地址</returns>
    public static OperateResult<string> PraseInovanceAddress(InovanceSeries series, string address, byte modbusCode)
    {
        return series switch
        {
            InovanceSeries.AM => PraseInovanceAMAddress(address, modbusCode),
            InovanceSeries.H3U => PraseInovanceH3UAddress(address, modbusCode),
            InovanceSeries.H5U => PraseInovanceH5UAddress(address, modbusCode),
            _ => new OperateResult<string>($"[{series}] Not supported series of plc"),
        };
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.PraseInovanceAddress(HslCommunication.Profinet.Inovance.InovanceSeries,System.String,System.Byte)" />
    public static OperateResult<string> PraseInovanceAMAddress(string address, byte modbusCode)
    {
        try
        {
            var text = string.Empty;
            var operateResult = CommunicationHelper.ExtractParameter(ref address, "s");
            if (operateResult.IsSuccess)
            {
                text = $"s={operateResult.Content};";
            }
            if (address.StartsWith("QX") || address.StartsWith("qx"))
            {
                return OperateResult.CreateSuccessResult(text + CalculateStartAddress(address.Substring(2)));
            }
            if (address.StartsWith("Q") || address.StartsWith("q"))
            {
                return OperateResult.CreateSuccessResult(text + CalculateStartAddress(address.Substring(1)));
            }
            if (address.StartsWith("IX") || address.StartsWith("ix"))
            {
                return OperateResult.CreateSuccessResult(text + "x=2;" + CalculateStartAddress(address.Substring(2)));
            }
            if (address.StartsWith("I") || address.StartsWith("i"))
            {
                return OperateResult.CreateSuccessResult(text + "x=2;" + CalculateStartAddress(address.Substring(1)));
            }
            if (address.StartsWith("MW") || address.StartsWith("mw"))
            {
                return OperateResult.CreateSuccessResult(text + address.Substring(2));
            }
            if (address.StartsWith("MD") || address.StartsWith("md"))
            {
                return OperateResult.CreateSuccessResult(text + Convert.ToInt32(address.Substring(2)) * 2);
            }
            if (address.StartsWith("MB") || address.StartsWith("mb"))
            {
                var num = Convert.ToInt32(address.Substring(2));
                if (num % 2 == 1)
                {
                    return new OperateResult<string>("Address[" + address + "] " + StringResources.Language.AddressOffsetEven);
                }
                return OperateResult.CreateSuccessResult(text + num / 2);
            }
            if (address.StartsWith("MX") || address.StartsWith("mx"))
            {
                if (modbusCode == 1 || modbusCode == 15 || modbusCode == 5)
                {
                    if (address.IndexOf('.') > 0)
                    {
                        var array = address.Substring(2).SplitDot();
                        var num2 = Convert.ToInt32(array[0]);
                        var num3 = Convert.ToInt32(array[1]);
                        return OperateResult.CreateSuccessResult(text + num2 / 2 + "." + (num2 % 2 * 8 + num3));
                    }
                    var num4 = Convert.ToInt32(address.Substring(2));
                    return OperateResult.CreateSuccessResult(text + num4 / 2 + ".0");
                }
                var num5 = Convert.ToInt32(address.Substring(2));
                return OperateResult.CreateSuccessResult(text + num5 / 2);
            }
            if (address.StartsWith("M") || address.StartsWith("m"))
            {
                return OperateResult.CreateSuccessResult(text + address.Substring(1));
            }
            if (modbusCode == 1 || modbusCode == 15 || modbusCode == 5)
            {
                if (address.StartsWith("SMX") || address.StartsWith("smx"))
                {
                    return OperateResult.CreateSuccessResult(text + $"x={modbusCode + 48};" + CalculateStartAddress(address.Substring(3)));
                }
                if (address.StartsWith("SM") || address.StartsWith("sm"))
                {
                    return OperateResult.CreateSuccessResult(text + $"x={modbusCode + 48};" + CalculateStartAddress(address.Substring(2)));
                }
            }
            else
            {
                if (address.StartsWith("SDW") || address.StartsWith("sdw"))
                {
                    return OperateResult.CreateSuccessResult(text + $"x={modbusCode + 48};" + address.Substring(3));
                }
                if (address.StartsWith("SD") || address.StartsWith("sd"))
                {
                    return OperateResult.CreateSuccessResult(text + $"x={modbusCode + 48};" + address.Substring(2));
                }
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }

    private static int CalculateH3UStartAddress(string address)
    {
        if (address.IndexOf('.') < 0)
        {
            return Convert.ToInt32(address, 8);
        }
        var array = address.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        return Convert.ToInt32(array[0], 8) * 8 + int.Parse(array[1]);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.PraseInovanceAddress(HslCommunication.Profinet.Inovance.InovanceSeries,System.String,System.Byte)" />
    public static OperateResult<string> PraseInovanceH3UAddress(string address, byte modbusCode)
    {
        try
        {
            var text = string.Empty;
            var operateResult = CommunicationHelper.ExtractParameter(ref address, "s");
            if (operateResult.IsSuccess)
            {
                text = $"s={operateResult.Content};";
            }
            if (modbusCode == 1 || modbusCode == 15 || modbusCode == 5)
            {
                if (ModbusHelper.TransAddressToModbus(text, address, new string[2] { "X", "Y" }, new int[2] { 63488, 64512 }, CalculateH3UStartAddress, out var newAddress))
                {
                    return OperateResult.CreateSuccessResult(newAddress);
                }
                if (ModbusHelper.TransAddressToModbus(text, address, new string[4] { "SM", "S", "T", "C" }, new int[4] { 9216, 57344, 61440, 62464 }, out var newAddress2))
                {
                    return OperateResult.CreateSuccessResult(newAddress2);
                }
                if (address.StartsWith("M") || address.StartsWith("m"))
                {
                    var num = Convert.ToInt32(address.Substring(1));
                    if (num >= 8000)
                    {
                        return OperateResult.CreateSuccessResult(text + (num - 8000 + 8000));
                    }
                    return OperateResult.CreateSuccessResult(text + num);
                }
                if (ModbusHelper.TransPointAddressToModbus(text, address, new string[3] { "D", "SD", "R" }, new int[3] { 0, 9216, 12288 }, out var newAddress3))
                {
                    return OperateResult.CreateSuccessResult(newAddress3);
                }
            }
            else
            {
                if (ModbusHelper.TransAddressToModbus(text, address, new string[4] { "D", "SD", "R", "T" }, new int[4] { 0, 9216, 12288, 61440 }, out var newAddress4))
                {
                    return OperateResult.CreateSuccessResult(newAddress4);
                }
                if (address.StartsWith("C", StringComparison.InvariantCultureIgnoreCase))
                {
                    var num2 = Convert.ToInt32(address.Substring(1));
                    if (num2 >= 200)
                    {
                        return OperateResult.CreateSuccessResult(text + ((num2 - 200) * 2 + 63232));
                    }
                    return OperateResult.CreateSuccessResult(text + (num2 + 62464));
                }
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.PraseInovanceAddress(HslCommunication.Profinet.Inovance.InovanceSeries,System.String,System.Byte)" />
    public static OperateResult<string> PraseInovanceH5UAddress(string address, byte modbusCode)
    {
        try
        {
            var station = string.Empty;
            var operateResult = CommunicationHelper.ExtractParameter(ref address, "s");
            if (operateResult.IsSuccess)
            {
                station = $"s={operateResult.Content};";
            }
            string newAddress4;
            if (modbusCode == 1 || modbusCode == 15 || modbusCode == 5)
            {
                if (ModbusHelper.TransAddressToModbus(station, address, new string[2] { "X", "Y" }, new int[2] { 63488, 64512 }, CalculateH3UStartAddress, out var newAddress))
                {
                    return OperateResult.CreateSuccessResult(newAddress);
                }
                if (ModbusHelper.TransAddressToModbus(station, address, new string[3] { "S", "B", "M" }, new int[3] { 57344, 12288, 0 }, out var newAddress2))
                {
                    return OperateResult.CreateSuccessResult(newAddress2);
                }
                if (ModbusHelper.TransPointAddressToModbus(station, address, new string[2] { "D", "R" }, new int[2] { 0, 12288 }, out var newAddress3))
                {
                    return OperateResult.CreateSuccessResult(newAddress3);
                }
            }
            else if (ModbusHelper.TransAddressToModbus(station, address, new string[2] { "D", "R" }, new int[2] { 0, 12288 }, out newAddress4))
            {
                return OperateResult.CreateSuccessResult(newAddress4);
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }
}
