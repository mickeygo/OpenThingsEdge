using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 三菱的FxLinks协议信息
/// </summary>
public class MelsecFxLinksAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 当前的地址类型信息
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public MelsecFxLinksAddress()
    {
    }

    /// <inheritdoc />
    public override void Parse(string address, ushort length)
    {
        base.Parse(address, length);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return TypeCode switch
        {
            "X" or "Y" => TypeCode + Convert.ToString(AddressStart, 8).PadLeft(AddressStart >= 10000 ? 6 : 4, '0'),
            _ => TypeCode + AddressStart.ToString("D" + ((AddressStart >= 10000 ? 7 : 5) - TypeCode?.Length)),
        };
    }

    public static OperateResult<MelsecFxLinksAddress> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从三菱FxLinks协议里面解析出实际的地址信息。
    /// </summary>
    /// <param name="address">三菱的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>解析结果信息</returns>
    public static OperateResult<MelsecFxLinksAddress> ParseFrom(string address, ushort length)
    {
        var melsecFxLinksAddress = new MelsecFxLinksAddress
        {
            Length = length
        };
        try
        {
            switch (address[0])
            {
                case 'X' or 'x':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 8);
                    melsecFxLinksAddress.TypeCode = "X";
                    break;
                case 'Y' or 'y':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 8);
                    melsecFxLinksAddress.TypeCode = "Y";
                    break;
                case 'M' or 'm':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    melsecFxLinksAddress.TypeCode = "M";
                    break;
                case 'S' or 's':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    melsecFxLinksAddress.TypeCode = "S";
                    break;
                case 'T' or 't':
                    if (address[1] is 'S' or 's')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        melsecFxLinksAddress.TypeCode = "TS";
                        break;
                    }
                    if (address[1] is 'N' or 'n')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        melsecFxLinksAddress.TypeCode = "TN";
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'C' or 'c':
                    if (address[1] is 'S' or 's')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        melsecFxLinksAddress.TypeCode = "CS";
                        break;
                    }
                    if (address[1] is 'N' or 'n')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        melsecFxLinksAddress.TypeCode = "CN";
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'D' or 'd':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    melsecFxLinksAddress.TypeCode = "D";
                    break;
                case 'R' or 'r':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    melsecFxLinksAddress.TypeCode = "R";
                    break;
                default:
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
            }
            return OperateResult.CreateSuccessResult(melsecFxLinksAddress);
        }
        catch (Exception ex)
        {
            return new OperateResult<MelsecFxLinksAddress>(GetUnsupportedAddressInfo(address, ex));
        }
    }
}
