using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 设备地址数据的信息，通常包含起始地址，数据类型，长度<br />
/// Device address data information, usually including the starting address, data type, length
/// </summary>
public class DeviceAddressDataBase
{
    /// <summary>
    /// 数字的起始地址，也就是偏移地址<br />
    /// The starting address of the number, which is the offset address
    /// </summary>
    public int AddressStart { get; set; }

    /// <summary>
    /// 读取的数据长度，单位是字节还是字取决于设备方<br />
    /// The length of the data read, the unit is byte or word depends on the device side
    /// </summary>
    public ushort Length { get; set; }

    /// <summary>
    /// 从指定的地址信息解析成真正的设备地址信息<br />
    /// Parsing from the specified address information into real device address information
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    public virtual void Parse(string address, ushort length)
    {
        AddressStart = int.Parse(address);
        Length = length;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AddressStart.ToString();
    }

    /// <summary>
    /// 获取地址不支持的描述信息<br />
    /// Get the description that the address is not supported
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="ex">解析地址时候的异常信息</param>
    /// <returns>地址不支持的信息</returns>
    public static string GetUnsupportedAddressInfo(string address, Exception ex = null)
    {
        if (ex == null)
        {
            return "Address[" + address + "]: " + StringResources.Language.NotSupportedDataType;
        }
        return "Parse [" + address + "] failed: " + ex.Message;
    }
}
