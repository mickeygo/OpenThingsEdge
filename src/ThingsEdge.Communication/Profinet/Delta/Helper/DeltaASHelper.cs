using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Delta.Helper;

/// <summary>
/// 台达AS300的辅助帮助类信息
/// </summary>
public static class DeltaASHelper
{
    private static int ParseDeltaBitAddress(string address)
    {
        var num = address.IndexOf('.');
        if (num > 0)
        {
            return Convert.ToInt32(address[..num]) * 16 + CommHelper.CalculateBitStartIndex(address[(num + 1)..]);
        }
        return Convert.ToInt32(address) * 16;
    }

    /// <summary>
    /// 根据台达AS300的PLC的地址，解析出转换后的modbus协议信息，适用AS300系列，当前的地址仍然支持站号指定，例如s=2;D100。
    /// </summary>
    /// <param name="address">台达plc的地址信息</param>
    /// <param name="modbusCode">原始的对应的modbus信息</param>
    /// <returns>还原后的modbus地址</returns>
    public static OperateResult<string> ParseDeltaASAddress(string address, byte modbusCode)
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
                if (address.StartsWith("SM") || address.StartsWith("sm"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(2)) + 16384));
                }
                if (address.StartsWith("HC") || address.StartsWith("hc"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(2)) + 64512));
                }
                if (address.StartsWith("S") || address.StartsWith("s"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 20480));
                }
                if (address.StartsWith("X") || address.StartsWith("x"))
                {
                    return OperateResult.CreateSuccessResult(text + "x=2;" + (ParseDeltaBitAddress(address.Substring(1)) + 24576));
                }
                if (address.StartsWith("Y") || address.StartsWith("y"))
                {
                    return OperateResult.CreateSuccessResult(text + (ParseDeltaBitAddress(address.Substring(1)) + 40960));
                }
                if (address.StartsWith("T") || address.StartsWith("t"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 57344));
                }
                if (address.StartsWith("C") || address.StartsWith("c"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 61440));
                }
                if (address.StartsWith("M") || address.StartsWith("m"))
                {
                    return OperateResult.CreateSuccessResult(text + Convert.ToInt32(address.Substring(1)));
                }
                if (address.StartsWith("D") && address.Contains("."))
                {
                    return OperateResult.CreateSuccessResult(text + address);
                }
            }
            else
            {
                if (address.StartsWith("SR") || address.StartsWith("sr"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(2)) + 49152));
                }
                if (address.StartsWith("HC") || address.StartsWith("hc"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(2)) + 64512));
                }
                if (address.StartsWith("D") || address.StartsWith("d"))
                {
                    return OperateResult.CreateSuccessResult(text + Convert.ToInt32(address.Substring(1)));
                }
                if (address.StartsWith("X") || address.StartsWith("x"))
                {
                    return OperateResult.CreateSuccessResult(text + "x=4;" + (Convert.ToInt32(address.Substring(1)) + 32768));
                }
                if (address.StartsWith("Y") || address.StartsWith("y"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 40960));
                }
                if (address.StartsWith("C") || address.StartsWith("c"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 61440));
                }
                if (address.StartsWith("T") || address.StartsWith("t"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 57344));
                }
                if (address.StartsWith("E") || address.StartsWith("e"))
                {
                    return OperateResult.CreateSuccessResult(text + (Convert.ToInt32(address.Substring(1)) + 65024));
                }
            }
            return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message);
        }
    }
}
