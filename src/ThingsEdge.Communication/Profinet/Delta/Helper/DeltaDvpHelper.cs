using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.Delta.Helper;

/// <summary>
/// 台达PLC的相关的帮助类，公共的地址解析的方法。
/// </summary>
public static class DeltaDvpHelper
{
    private static int TransDAdressToModbusAddress(string address)
    {
        var num = Convert.ToInt32(address);
        if (num >= 4096)
        {
            return num - 4096 + 36864;
        }
        return num + 4096;
    }

    /// <summary>
    /// 根据台达PLC的地址，解析出转换后的modbus协议信息，适用DVP系列，当前的地址仍然支持站号指定，例如s=2;D100。
    /// </summary>
    /// <param name="address">台达plc的地址信息</param>
    /// <param name="modbusCode">原始的对应的modbus信息</param>
    /// <returns>还原后的modbus地址</returns>
    public static OperateResult<string> ParseDeltaDvpAddress(string address, byte modbusCode)
    {
        try
        {
            var text = string.Empty;
            var operateResult = CommHelper.ExtractParameter(ref address, "s");
            if (operateResult.IsSuccess)
            {
                text = $"s={operateResult.Content};";
            }
            if (modbusCode == 1 || modbusCode == 15 || modbusCode == 5)
            {
                if (address.StartsWithAndNumber("S"))
                {
                    return OperateResult.CreateSuccessResult(text + Convert.ToInt32(address.Substring(1)));
                }
                if (address.StartsWithAndNumber("X"))
                {
                    return OperateResult.CreateSuccessResult(text + "x=2;" + (Convert.ToInt32(address.Substring(1), 8) + 1024));
                }
                if (address.StartsWithAndNumber("Y"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1), 8) + 1280));
                }
                if (address.StartsWithAndNumber("T"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 1536));
                }
                if (address.StartsWithAndNumber("C"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 3584));
                }
                if (address.StartsWithAndNumber("M"))
                {
                    var num = Convert.ToInt32(address[1..]);
                    if (num >= 1536)
                    {
                        return OperateResult.CreateSuccessResult(text + (num - 1536 + 45056));
                    }
                    return OperateResult.CreateSuccessResult(text + (num + 2048));
                }
                if (ModbusHelper.TransPointAddressToModbus(text, address, ["D"], new int[1], TransDAdressToModbusAddress, out var newAddress))
                {
                    return OperateResult.CreateSuccessResult(newAddress);
                }
            }
            else
            {
                if (address.StartsWithAndNumber("D"))
                {
                    return OperateResult.CreateSuccessResult(text + TransDAdressToModbusAddress(address.Substring(1)));
                }
                if (address.StartsWithAndNumber("C"))
                {
                    var num2 = Convert.ToInt32(address.Substring(1));
                    if (num2 >= 200)
                    {
                        return OperateResult.CreateSuccessResult(text + (num2 - 200 + 3784));
                    }
                    return OperateResult.CreateSuccessResult(text + (num2 + 3584));
                }
                if (address.StartsWithAndNumber("T"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 1536));
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
    /// 读取台达PLC的bool变量，重写了读M地址时，跨区域读1536地址时，将会分割多次读取
    /// </summary>
    /// <param name="readBoolFunc">底层基础的读取方法</param>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>读取的结果</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(Func<string, ushort, Task<OperateResult<bool[]>>> readBoolFunc, string address, ushort length)
    {
        var station = string.Empty;
        var stationPara = CommHelper.ExtractParameter(ref address, "s");
        if (stationPara.IsSuccess)
        {
            station = $"s={stationPara.Content};";
        }
        if (address.StartsWith('M') && int.TryParse(address[1..], out var add) && add < 1536 && add + length > 1536)
        {
            var len1 = (ushort)(1536 - add);
            var len2 = (ushort)(length - len1);
            var read1 = await readBoolFunc(station + address, len1).ConfigureAwait(false);
            if (!read1.IsSuccess)
            {
                return read1;
            }
            var read2 = await readBoolFunc(station + "M1536", len2).ConfigureAwait(false);
            if (!read2.IsSuccess)
            {
                return read2;
            }
            return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(read1.Content!, read2.Content!));
        }
        return await readBoolFunc(address, length).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入台达PLC的原始字节数据，当发现是D类型的数据，并且地址出现跨4096时，进行切割写入操作
    /// </summary>
    /// <param name="writeBoolFunc">底层的写入操作方法</param>
    /// <param name="address">PLC的起始地址信息</param>
    /// <param name="value">等待写入的数据信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(Func<string, bool[], Task<OperateResult>> writeBoolFunc, string address, bool[] value)
    {
        var station = string.Empty;
        var stationPara = CommHelper.ExtractParameter(ref address, "s");
        if (stationPara.IsSuccess)
        {
            station = $"s={stationPara.Content};";
        }
        if (address.StartsWith('M') && int.TryParse(address[1..], out var add) && add < 1536 && add + value.Length > 1536)
        {
            var len1 = (ushort)(1536 - add);
            var write1 = await writeBoolFunc(station + address, value.SelectBegin(len1)).ConfigureAwait(false);
            if (!write1.IsSuccess)
            {
                return write1;
            }
            var write2 = await writeBoolFunc(station + "M1536", value.RemoveBegin(len1)).ConfigureAwait(false);
            if (!write2.IsSuccess)
            {
                return write2;
            }
            return OperateResult.CreateSuccessResult();
        }
        return await writeBoolFunc(address, value).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取台达PLC的原始字节变量，重写了读D地址时，跨区域读4096地址时，将会分割多次读取
    /// </summary>
    /// <param name="readFunc">底层基础的读取方法</param>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>读取的结果</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(Func<string, ushort, Task<OperateResult<byte[]>>> readFunc, string address, ushort length)
    {
        var station = string.Empty;
        var stationPara = CommHelper.ExtractParameter(ref address, "s");
        if (stationPara.IsSuccess)
        {
            station = $"s={stationPara.Content};";
        }
        if (address.StartsWith('D') && int.TryParse(address[1..], out var add) && add < 4096 && add + length > 4096)
        {
            var len1 = (ushort)(4096 - add);
            var len2 = (ushort)(length - len1);
            var read1 = await readFunc(station + address, len1).ConfigureAwait(false);
            if (!read1.IsSuccess)
            {
                return read1;
            }
            var read2 = await readFunc(station + "D4096", len2).ConfigureAwait(false);
            if (!read2.IsSuccess)
            {
                return read2;
            }
            return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(read1.Content!, read2.Content!));
        }
        return await readFunc(address, length).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入台达PLC的原始字节数据，当发现是D类型的数据，并且地址出现跨4096时，进行切割写入操作
    /// </summary>
    /// <param name="writeFunc">底层的写入操作方法</param>
    /// <param name="address">PLC的起始地址信息</param>
    /// <param name="value">等待写入的数据信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(Func<string, byte[], Task<OperateResult>> writeFunc, string address, byte[] value)
    {
        var station = string.Empty;
        var stationPara = CommHelper.ExtractParameter(ref address, "s");
        if (stationPara.IsSuccess)
        {
            station = $"s={stationPara.Content};";
        }
        if (address.StartsWith('D') && int.TryParse(address[1..], out var add) && add < 4096 && add + value.Length / 2 > 4096)
        {
            var len1 = (ushort)(4096 - add);
            var write1 = await writeFunc(station + address, value.SelectBegin(len1 * 2)).ConfigureAwait(false);
            if (!write1.IsSuccess)
            {
                return write1;
            }
            var write2 = await writeFunc(station + "D4096", value.RemoveBegin(len1 * 2)).ConfigureAwait(false);
            if (!write2.IsSuccess)
            {
                return write2;
            }
            return OperateResult.CreateSuccessResult();
        }
        return await writeFunc(address, value).ConfigureAwait(false);
    }
}
