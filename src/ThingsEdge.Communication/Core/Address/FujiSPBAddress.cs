using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;
using ThingsEdge.Communication.Profinet.Fuji;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// FujiSPB的地址信息，可以携带数据类型，起始地址操作。
/// </summary>
public class FujiSPBAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 数据的类型代码
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// 当是位地址的时候，用于标记的信息
    /// </summary>
    public int BitIndex { get; set; }

    /// <summary>
    /// 获取读写字数据的时候的地址信息内容
    /// </summary>
    /// <returns>报文信息</returns>
    public string GetWordAddress()
    {
        return TypeCode + FujiSPBHelper.AnalysisIntegerAddress(AddressStart);
    }

    /// <summary>
    /// 获取命令，写入字地址的某一位的命令内容
    /// </summary>
    /// <returns>报文信息</returns>
    public string GetWriteBoolAddress()
    {
        var num = AddressStart * 2;
        var num2 = BitIndex;
        if (num2 >= 8)
        {
            num++;
            num2 -= 8;
        }
        return $"{TypeCode}{FujiSPBHelper.AnalysisIntegerAddress(num)}{num2:X2}";
    }

    /// <summary>
    /// 按照位为单位获取相关的索引信息
    /// </summary>
    /// <returns>位数据信息</returns>
    public int GetBitIndex()
    {
        return AddressStart * 16 + BitIndex;
    }

    /// <summary>
    /// 从实际的Fuji的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual Fuji address
    /// </summary>
    /// <param name="address">富士的地址数据信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<FujiSPBAddress> ParseFrom(string address)
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
    public static OperateResult<FujiSPBAddress> ParseFrom(string address, ushort length)
    {
        var fujiSPBAddress = new FujiSPBAddress
        {
            Length = length
        };
        try
        {
            fujiSPBAddress.BitIndex = CommunicationHelper.GetBitIndexInformation(ref address);
            switch (address[0])
            {
                case 'X' or 'x':
                    fujiSPBAddress.TypeCode = "01";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'Y' or 'y':
                    fujiSPBAddress.TypeCode = "00";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'M' or 'm':
                    fujiSPBAddress.TypeCode = "02";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'L' or 'l':
                    fujiSPBAddress.TypeCode = "03";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'T' or 't':
                    if (address[1] is 'N' or 'n')
                    {
                        fujiSPBAddress.TypeCode = "0A";
                        fujiSPBAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        break;
                    }
                    if (address[1] is 'C' or 'c')
                    {
                        fujiSPBAddress.TypeCode = "04";
                        fujiSPBAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'C' or 'c':
                    if (address[1] is 'N' or 'n')
                    {
                        fujiSPBAddress.TypeCode = "0B";
                        fujiSPBAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        break;
                    }
                    if (address[1] is 'C' or 'c')
                    {
                        fujiSPBAddress.TypeCode = "05";
                        fujiSPBAddress.AddressStart = Convert.ToUInt16(address[2..], 10);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'D' or 'd':
                    fujiSPBAddress.TypeCode = "0C";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'R' or 'r':
                    fujiSPBAddress.TypeCode = "0D";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                case 'W' or 'w':
                    fujiSPBAddress.TypeCode = "0E";
                    fujiSPBAddress.AddressStart = Convert.ToUInt16(address[1..], 10);
                    break;
                default:
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<FujiSPBAddress>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(fujiSPBAddress);
    }
}
