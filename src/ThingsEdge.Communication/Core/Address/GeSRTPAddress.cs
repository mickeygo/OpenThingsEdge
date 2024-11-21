using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// GE的SRTP协议的地址内容，主要包含一个数据代码信息，还有静态的解析地址的方法。
/// </summary>
public class GeSRTPAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置等待读取的数据的代码。
    /// </summary>
    public byte DataCode { get; set; }

    public override void Parse(string address, ushort length)
    {
        var operateResult = ParseFrom(address, length, isBit: false);
        if (operateResult.IsSuccess)
        {
            AddressStart = operateResult.Content.AddressStart;
            Length = operateResult.Content.Length;
            DataCode = operateResult.Content.DataCode;
        }
    }

    public static OperateResult<GeSRTPAddress> ParseFrom(string address, bool isBit)
    {
        return ParseFrom(address, 0, isBit);
    }

    /// <summary>
    /// 从GE的地址里，解析出实际的带数据码的 <see cref="GeSRTPAddress" /> 地址信息，起始地址会自动减一，和实际的地址相匹配
    /// </summary>
    /// <param name="address">实际的地址数据</param>
    /// <param name="length">读取的长度信息</param>
    /// <param name="isBit">是否位操作</param>
    /// <returns>是否成功的GE地址对象</returns>
    public static OperateResult<GeSRTPAddress> ParseFrom(string address, ushort length, bool isBit)
    {
        var geSRTPAddress = new GeSRTPAddress();
        try
        {
            geSRTPAddress.Length = length;
            if (address.StartsWith("AI", StringComparison.OrdinalIgnoreCase))
            {
                if (isBit)
                {
                    return new OperateResult<GeSRTPAddress>(StringResources.Language.GeSRTPNotSupportBitReadWrite);
                }
                geSRTPAddress.DataCode = 10;
                geSRTPAddress.AddressStart = Convert.ToInt32(address[2..]);
            }
            else if (address.StartsWith("AQ", StringComparison.OrdinalIgnoreCase))
            {
                if (isBit)
                {
                    return new OperateResult<GeSRTPAddress>(StringResources.Language.GeSRTPNotSupportBitReadWrite);
                }
                geSRTPAddress.DataCode = 12;
                geSRTPAddress.AddressStart = Convert.ToInt32(address[2..]);
            }
            else if (address.StartsWith('R') || address.StartsWith('r'))
            {
                if (isBit)
                {
                    return new OperateResult<GeSRTPAddress>(StringResources.Language.GeSRTPNotSupportBitReadWrite);
                }
                geSRTPAddress.DataCode = 8;
                geSRTPAddress.AddressStart = Convert.ToInt32(address[1..]);
            }
            else if (address.StartsWith("SA", StringComparison.OrdinalIgnoreCase))
            {
                geSRTPAddress.DataCode = (byte)(isBit ? 78 : 24);
                geSRTPAddress.AddressStart = Convert.ToInt32(address[2..]);
            }
            else if (address.StartsWith("SB", StringComparison.OrdinalIgnoreCase))
            {
                geSRTPAddress.DataCode = (byte)(isBit ? 80 : 26);
                geSRTPAddress.AddressStart = Convert.ToInt32(address[2..]);
            }
            else if (address.StartsWith("SC", StringComparison.OrdinalIgnoreCase))
            {
                geSRTPAddress.DataCode = (byte)(isBit ? 82 : 28);
                geSRTPAddress.AddressStart = Convert.ToInt32(address[2..]);
            }
            else
            {
                if (address[0] is 'I' or 'i')
                {
                    geSRTPAddress.DataCode = (byte)(isBit ? 70 : 16);
                }
                else if (address[0] is 'Q' or 'q')
                {
                    geSRTPAddress.DataCode = (byte)(isBit ? 72 : 18);
                }
                else if (address[0] is 'M' or 'm')
                {
                    geSRTPAddress.DataCode = (byte)(isBit ? 76 : 22);
                }
                else if (address[0] is 'T' or 't')
                {
                    geSRTPAddress.DataCode = (byte)(isBit ? 74 : 20);
                }
                else if (address[0] is 'S' or 's')
                {
                    geSRTPAddress.DataCode = (byte)(isBit ? 84 : 30);
                }
                else
                {
                    if (address[0] is not 'G' and not 'g')
                    {
                        throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                    }
                    geSRTPAddress.DataCode = (byte)(isBit ? 86 : 56);
                }
                geSRTPAddress.AddressStart = Convert.ToInt32(address[1..]);
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<GeSRTPAddress>(GetUnsupportedAddressInfo(address, ex));
        }
        if (geSRTPAddress.AddressStart == 0)
        {
            return new OperateResult<GeSRTPAddress>(StringResources.Language.GeSRTPAddressCannotBeZero);
        }
        if (geSRTPAddress.AddressStart > 0)
        {
            geSRTPAddress.AddressStart--;
        }
        return OperateResult.CreateSuccessResult(geSRTPAddress);
    }
}
