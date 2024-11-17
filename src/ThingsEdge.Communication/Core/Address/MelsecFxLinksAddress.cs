using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 三菱的FxLinks协议信息
/// </summary>
public class MelsecFxLinksAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 当前的地址类型信息
    /// </summary>
    public string TypeCode { get; set; }

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
        switch (TypeCode)
        {
            case "X":
            case "Y":
                return TypeCode + Convert.ToString(AddressStart, 8).PadLeft(AddressStart >= 10000 ? 6 : 4, '0');
            default:
                return TypeCode + AddressStart.ToString("D" + ((AddressStart >= 10000 ? 7 : 5) - TypeCode.Length));
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Address.MelsecFxLinksAddress.Parse(System.String,System.UInt16)" />
    public static OperateResult<MelsecFxLinksAddress> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从三菱FxLinks协议里面解析出实际的地址信息
    /// </summary>
    /// <param name="address">三菱的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>解析结果信息</returns>
    public static OperateResult<MelsecFxLinksAddress> ParseFrom(string address, ushort length)
    {
        var melsecFxLinksAddress = new MelsecFxLinksAddress();
        melsecFxLinksAddress.Length = length;
        try
        {
            switch (address[0])
            {
                case 'X':
                case 'x':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 8);
                    melsecFxLinksAddress.TypeCode = "X";
                    break;
                case 'Y':
                case 'y':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 8);
                    melsecFxLinksAddress.TypeCode = "Y";
                    break;
                case 'M':
                case 'm':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 10);
                    melsecFxLinksAddress.TypeCode = "M";
                    break;
                case 'S':
                case 's':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 10);
                    melsecFxLinksAddress.TypeCode = "S";
                    break;
                case 'T':
                case 't':
                    if (address[1] == 'S' || address[1] == 's')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(2), 10);
                        melsecFxLinksAddress.TypeCode = "TS";
                        break;
                    }
                    if (address[1] == 'N' || address[1] == 'n')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(2), 10);
                        melsecFxLinksAddress.TypeCode = "TN";
                        break;
                    }
                    throw new Exception(StringResources.Language.NotSupportedDataType);
                case 'C':
                case 'c':
                    if (address[1] == 'S' || address[1] == 's')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(2), 10);
                        melsecFxLinksAddress.TypeCode = "CS";
                        break;
                    }
                    if (address[1] == 'N' || address[1] == 'n')
                    {
                        melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(2), 10);
                        melsecFxLinksAddress.TypeCode = "CN";
                        break;
                    }
                    throw new Exception(StringResources.Language.NotSupportedDataType);
                case 'D':
                case 'd':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 10);
                    melsecFxLinksAddress.TypeCode = "D";
                    break;
                case 'R':
                case 'r':
                    melsecFxLinksAddress.AddressStart = Convert.ToUInt16(address.Substring(1), 10);
                    melsecFxLinksAddress.TypeCode = "R";
                    break;
                default:
                    throw new Exception(StringResources.Language.NotSupportedDataType);
            }
            return OperateResult.CreateSuccessResult(melsecFxLinksAddress);
        }
        catch (Exception ex)
        {
            return new OperateResult<MelsecFxLinksAddress>(GetUnsupportedAddressInfo(address, ex));
        }
    }
}
