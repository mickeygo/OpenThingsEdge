using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 富士SPH地址类对象
/// </summary>
public class FujiSPHAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 数据的类型代码
    /// </summary>
    public byte TypeCode { get; set; }

    /// <summary>
    /// 当前地址的位索引信息
    /// </summary>
    public int BitIndex { get; set; }

    /// <summary>
    /// 从实际的Fuji的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual Fuji address
    /// </summary>
    /// <param name="address">富士的地址数据信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<FujiSPHAddress> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从实际的Fuji的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual Fuji address
    /// </summary>
    /// <param name="address">富士的地址数据信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<FujiSPHAddress> ParseFrom(string address, ushort length)
    {
        var fujiSPHAddress = new FujiSPHAddress();
        try
        {
            switch (address[0])
            {
                case 'M' or 'm':
                    {
                        var array2 = address.SplitDot();
                        switch (int.Parse(array2[0][1..]))
                        {
                            case 1:
                                fujiSPHAddress.TypeCode = 2;
                                break;
                            case 3:
                                fujiSPHAddress.TypeCode = 4;
                                break;
                            case 10:
                                fujiSPHAddress.TypeCode = 8;
                                break;
                            default:
                                throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                        }
                        fujiSPHAddress.AddressStart = Convert.ToInt32(array2[1]);
                        if (array2.Length > 2)
                        {
                            fujiSPHAddress.BitIndex = CommunicationHelper.CalculateBitStartIndex(array2[2]);
                        }
                        break;
                    }
                case 'I' or 'Q' or 'i' or 'q':
                    {
                        var array = address.SplitDot();
                        fujiSPHAddress.TypeCode = 1;
                        fujiSPHAddress.AddressStart = Convert.ToInt32(array[0][1..]);
                        if (array.Length > 1)
                        {
                            fujiSPHAddress.BitIndex = CommunicationHelper.CalculateBitStartIndex(array[1]);
                        }
                        break;
                    }
                default:
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<FujiSPHAddress>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(fujiSPHAddress);
    }
}
