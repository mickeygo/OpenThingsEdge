using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 信捷内部协议的地址类对象<br />
/// The address class object of Xinjie internal protocol
/// </summary>
public class XinJEAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置等待读取的数据的代码<br />
    /// Get or set the code of the data waiting to be read
    /// </summary>
    public byte DataCode { get; set; }

    /// <summary>
    /// 获取或设置当前的站号信息<br />
    /// Get or set the current station number information
    /// </summary>
    public byte Station { get; set; }

    /// <summary>
    /// 获取或设置协议升级时候的临界地址信息<br />
    /// Get or set the critical address information when the protocol is upgraded
    /// </summary>
    public int CriticalAddress { get; set; }

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// instantiate a default object
    /// </summary>
    public XinJEAddress()
    {
    }

    /// <summary>
    /// 指定类型，地址偏移，临界地址来实例化一个对象<br />
    /// Specify the type, address offset, and critical address to instantiate an object
    /// </summary>
    /// <param name="dataCode">数据的类型代号</param>
    /// <param name="address">偏移地址信息</param>
    /// <param name="criticalAddress">临界地址信息</param>
    /// <param name="station">站号信息</param>
    public XinJEAddress(byte dataCode, int address, int criticalAddress, byte station)
    {
        DataCode = dataCode;
        AddressStart = address;
        CriticalAddress = criticalAddress;
        Station = station;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AddressStart.ToString();
    }

    /// <summary>
    /// 从实际的信捷PLC的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual XinJE address
    /// </summary>
    /// <param name="address">信捷的地址数据信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <param name="defaultStation">默认的站号信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<XinJEAddress> ParseFrom(string address, ushort length, byte defaultStation)
    {
        var operateResult = ParseFrom(address, defaultStation);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<XinJEAddress>(operateResult);
        }
        operateResult.Content.Length = length;
        return OperateResult.CreateSuccessResult(operateResult.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Address.XinJEAddress.ParseFrom(System.String,System.UInt16,System.Byte)" />
    public static OperateResult<XinJEAddress> ParseFrom(string address, byte defaultStation)
    {
        try
        {
            var station = (byte)CommHelper.ExtractParameter(ref address, "s", defaultStation);
            if (address.StartsWith("HSCD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(139, int.Parse(address.Substring(4)), int.MaxValue, station));
            }
            if (address.StartsWith("ETD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(133, int.Parse(address.Substring(3)), 0, station));
            }
            if (address.StartsWith("HSD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(140, int.Parse(address.Substring(3)), 1024, station));
            }
            if (address.StartsWith("HTD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(137, int.Parse(address.Substring(3)), 1024, station));
            }
            if (address.StartsWith("HCD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(138, int.Parse(address.Substring(3)), 1024, station));
            }
            if (address.StartsWith("SFD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(142, int.Parse(address.Substring(3)), 4096, station));
            }
            if (address.StartsWith("HSC"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(12, int.Parse(address.Substring(3)), int.MaxValue, station));
            }
            if (address.StartsWith("SD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(131, int.Parse(address.Substring(2)), 4096, station));
            }
            if (address.StartsWith("TD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(129, int.Parse(address.Substring(2)), 4096, station));
            }
            if (address.StartsWith("CD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(130, int.Parse(address.Substring(2)), 4096, station));
            }
            if (address.StartsWith("HD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(136, int.Parse(address.Substring(2)), 6144, station));
            }
            if (address.StartsWith("FD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(141, int.Parse(address.Substring(2)), 8192, station));
            }
            if (address.StartsWith("ID"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(134, int.Parse(address.Substring(2)), 0, station));
            }
            if (address.StartsWith("QD"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(135, int.Parse(address.Substring(2)), 0, station));
            }
            if (address.StartsWith("SM"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(13, int.Parse(address.Substring(2)), 4096, station));
            }
            if (address.StartsWith("ET"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(7, int.Parse(address.Substring(2)), 0, station));
            }
            if (address.StartsWith("HM"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(8, int.Parse(address.Substring(2)), 6144, station));
            }
            if (address.StartsWith("HS"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(9, int.Parse(address.Substring(2)), int.MaxValue, station));
            }
            if (address.StartsWith("HT"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(10, int.Parse(address.Substring(2)), 1024, station));
            }
            if (address.StartsWith("HC"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(11, int.Parse(address.Substring(2)), 1024, station));
            }
            if (address.StartsWith("D"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(128, int.Parse(address.Substring(1)), 20480, station));
            }
            if (address.StartsWith("M"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(3, int.Parse(address.Substring(1)), 20480, station));
            }
            if (address.StartsWith("T"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(5, int.Parse(address.Substring(1)), 4096, station));
            }
            if (address.StartsWith("C"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(6, int.Parse(address.Substring(1)), 4096, station));
            }
            if (address.StartsWith("Y"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(2, Convert.ToInt32(address.Substring(1), 8), int.MaxValue, station));
            }
            if (address.StartsWith("X"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(1, Convert.ToInt32(address.Substring(1), 8), int.MaxValue, station));
            }
            if (address.StartsWith("S"))
            {
                return OperateResult.CreateSuccessResult(new XinJEAddress(4, int.Parse(address.Substring(1)), 8000, station));
            }
            throw new Exception(StringResources.Language.NotSupportedDataType);
        }
        catch (Exception ex)
        {
            return new OperateResult<XinJEAddress>(GetUnsupportedAddressInfo(address, ex));
        }
    }
}
