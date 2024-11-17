using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 西门子的地址数据信息，主要包含数据代码，DB块，偏移地址（偏移地址对于不是CT类型而已，是位为单位的），当处于写入时，Length无效<br />
/// Address data information of Siemens, mainly including data code, DB block, offset address, when writing, Length is invalid
/// </summary>
public class S7AddressData : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置等待读取的数据的代码<br />
    /// Get or set the code of the data waiting to be read
    /// </summary>
    public byte DataCode { get; set; }

    /// <summary>
    /// 获取或设置PLC的DB块数据信息<br />
    /// Get or set PLC DB data information
    /// </summary>
    public ushort DbBlock { get; set; }

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public S7AddressData()
    {
    }

    /// <summary>
    /// 从另一个 <see cref="T:HslCommunication.Core.Address.S7AddressData" /> 对象进行实例化<br />
    /// Instantiate from another <see cref="T:HslCommunication.Core.Address.S7AddressData" /> object
    /// </summary>
    /// <param name="s7Address">S7地址对象</param>
    public S7AddressData(S7AddressData s7Address)
    {
        AddressStart = s7Address.AddressStart;
        Length = s7Address.Length;
        DbBlock = s7Address.DbBlock;
        DataCode = s7Address.DataCode;
    }

    /// <summary>
    /// 从指定的地址信息解析成真正的设备地址信息
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    public override void Parse(string address, ushort length)
    {
        var operateResult = ParseFrom(address, length);
        if (operateResult.IsSuccess)
        {
            AddressStart = operateResult.Content.AddressStart;
            Length = operateResult.Content.Length;
            DataCode = operateResult.Content.DataCode;
            DbBlock = operateResult.Content.DbBlock;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (DataCode == 31)
        {
            return "T" + AddressStart;
        }
        if (DataCode == 30)
        {
            return "C" + AddressStart;
        }
        if (DataCode == 5)
        {
            return "SM" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 6)
        {
            return "AI" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 7)
        {
            return "AQ" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 128)
        {
            return "P" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 129)
        {
            return "I" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 130)
        {
            return "Q" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 131)
        {
            return "M" + GetActualStringAddress(AddressStart);
        }
        if (DataCode == 132)
        {
            return "DB" + DbBlock + "." + GetActualStringAddress(AddressStart);
        }
        return AddressStart.ToString();
    }

    private static string GetActualStringAddress(int addressStart)
    {
        if (addressStart % 8 == 0)
        {
            return (addressStart / 8).ToString();
        }
        return $"{addressStart / 8}.{addressStart % 8}";
    }

    /// <summary>
    /// 计算特殊的地址信息<br />
    /// Calculate Special Address information
    /// </summary>
    /// <param name="address">字符串地址 -&gt; String address</param>
    /// <param name="isCT">是否是定时器和计数器的地址</param>
    /// <returns>实际值 -&gt; Actual value</returns>
    public static int CalculateAddressStarted(string address, bool isCT = false)
    {
        if (address.IndexOf('.') < 0)
        {
            if (isCT)
            {
                return Convert.ToInt32(address);
            }
            return Convert.ToInt32(address) * 8;
        }
        var array = address.Split('.');
        return Convert.ToInt32(array[0]) * 8 + Convert.ToInt32(array[1]);
    }

    /// <summary>
    /// 从实际的西门子的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual Siemens address
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<S7AddressData> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从实际的西门子的地址里面解析出地址对象<br />
    /// Resolve the address object from the actual Siemens address
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<S7AddressData> ParseFrom(string address, ushort length)
    {
        var s7AddressData = new S7AddressData();
        try
        {
            address = address.ToUpper();
            s7AddressData.Length = length;
            s7AddressData.DbBlock = 0;
            if (address.StartsWith("SM"))
            {
                s7AddressData.DataCode = 5;
                if (address.StartsWith("SMX") || address.StartsWith("SMB") || address.StartsWith("SMW") || address.StartsWith("SMD"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
            }
            else if (address.StartsWith("AI"))
            {
                s7AddressData.DataCode = 6;
                if (address.StartsWith("AIX") || address.StartsWith("AIB") || address.StartsWith("AIW") || address.StartsWith("AID"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
            }
            else if (address.StartsWith("AQ"))
            {
                s7AddressData.DataCode = 7;
                if (address.StartsWith("AQX") || address.StartsWith("AQB") || address.StartsWith("AQW") || address.StartsWith("AQD"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
            }
            else if (address[0] == 'P')
            {
                s7AddressData.DataCode = 128;
                if (address.StartsWith("PIX") || address.StartsWith("PIB") || address.StartsWith("PIW") || address.StartsWith("PID") || address.StartsWith("PQX") || address.StartsWith("PQB") || address.StartsWith("PQW") || address.StartsWith("PQD"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                }
                else if (address.StartsWith("PI") || address.StartsWith("PQ"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                }
            }
            else if (address[0] == 'I')
            {
                s7AddressData.DataCode = 129;
                if (address.StartsWith("IX") || address.StartsWith("IB") || address.StartsWith("IW") || address.StartsWith("ID"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                }
            }
            else if (address[0] == 'Q')
            {
                s7AddressData.DataCode = 130;
                if (address.StartsWith("QX") || address.StartsWith("QB") || address.StartsWith("QW") || address.StartsWith("QD"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                }
            }
            else if (address[0] == 'M')
            {
                s7AddressData.DataCode = 131;
                if (address.StartsWith("MX") || address.StartsWith("MB") || address.StartsWith("MW") || address.StartsWith("MD"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                }
            }
            else if (address[0] == 'D' || address.Substring(0, 2) == "DB")
            {
                s7AddressData.DataCode = 132;
                var array = address.Split('.');
                if (address[1] == 'B')
                {
                    s7AddressData.DbBlock = Convert.ToUInt16(array[0].Substring(2));
                }
                else
                {
                    s7AddressData.DbBlock = Convert.ToUInt16(array[0].Substring(1));
                }
                var text = address.Substring(address.IndexOf('.') + 1);
                if (text.StartsWith("DBX") || text.StartsWith("DBB") || text.StartsWith("DBW") || text.StartsWith("DBD"))
                {
                    text = text.Substring(3);
                }
                s7AddressData.AddressStart = CalculateAddressStarted(text);
            }
            else if (address[0] == 'T')
            {
                s7AddressData.DataCode = 31;
                s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), isCT: true);
            }
            else if (address[0] == 'C')
            {
                s7AddressData.DataCode = 30;
                s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), isCT: true);
            }
            else
            {
                if (address[0] != 'V')
                {
                    throw new Exception(StringResources.Language.NotSupportedDataType);
                }
                s7AddressData.DataCode = 132;
                s7AddressData.DbBlock = 1;
                if (address.StartsWith("VB") || address.StartsWith("VW") || address.StartsWith("VD") || address.StartsWith("VX"))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                }
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<S7AddressData>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(s7AddressData);
    }
}
