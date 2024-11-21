using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 西门子的地址数据信息，主要包含数据代码，DB块，偏移地址（偏移地址对于不是CT类型而已，是位为单位的），当处于写入时，Length无效。
/// </summary>
public class S7AddressData : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置等待读取的数据的代码。
    /// </summary>
    public byte DataCode { get; set; }

    /// <summary>
    /// 获取或设置PLC的DB块数据信息。
    /// </summary>
    public ushort DbBlock { get; set; }

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public S7AddressData()
    {
    }

    /// <summary>
    /// 从另一个对象进行实例化。
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
    /// 从指定的地址信息解析成真正的设备地址信息。
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
    /// 计算特殊的地址信息。
    /// </summary>
    /// <param name="address">字符串地址</param>
    /// <param name="isCT">是否是定时器和计数器的地址</param>
    /// <returns>实际值</returns>
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
    /// 从实际的西门子的地址里面解析出地址对象。
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<S7AddressData> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从实际的西门子的地址里面解析出地址对象。
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
                if (address.StartsWith(["SMX", "SMB", "SMW", "SMD"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[3..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
            }
            else if (address.StartsWith("AI"))
            {
                s7AddressData.DataCode = 6;
                if (address.StartsWith(["AIX", "AIB", "AIW", "AID"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[3..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
            }
            else if (address.StartsWith("AQ"))
            {
                s7AddressData.DataCode = 7;
                if (address.StartsWith(["AQX", "AQB", "AQW", "AQD"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[3..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
            }
            else if (address[0] == 'P')
            {
                s7AddressData.DataCode = 128;
                if (address.StartsWith(["PIX", "PIB", "PIW", "PID", "PQX", "PQB", "PQW", "PQD"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[3..]);
                }
                else if (address.StartsWith(["PI", "PQ"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[1..]);
                }
            }
            else if (address[0] == 'I')
            {
                s7AddressData.DataCode = 129;
                if (address.StartsWith(["IX", "IB", "IW", "ID"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[1..]);
                }
            }
            else if (address[0] == 'Q')
            {
                s7AddressData.DataCode = 130;
                if (address.StartsWith(["QX", "QB", "QW", "QD"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[1..]);
                }
            }
            else if (address[0] == 'M')
            {
                s7AddressData.DataCode = 131;
                if (address.StartsWith(["MX", "MB", "MW", "MD"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[1..]);
                }
            }
            else if (address[0] == 'D' || address[..2] == "DB")
            {
                s7AddressData.DataCode = 132;
                var array = address.Split('.');
                if (address[1] == 'B')
                {
                    s7AddressData.DbBlock = Convert.ToUInt16(array[0][2..]);
                }
                else
                {
                    s7AddressData.DbBlock = Convert.ToUInt16(array[0][1..]);
                }
                var text = address[(address.IndexOf('.') + 1)..];
                if (text.StartsWith(["DBX", "DBB", "DBW", "DBD"]))
                {
                    text = text[3..];
                }
                s7AddressData.AddressStart = CalculateAddressStarted(text);
            }
            else if (address[0] == 'T')
            {
                s7AddressData.DataCode = 31;
                s7AddressData.AddressStart = CalculateAddressStarted(address[1..], isCT: true);
            }
            else if (address[0] == 'C')
            {
                s7AddressData.DataCode = 30;
                s7AddressData.AddressStart = CalculateAddressStarted(address[1..], isCT: true);
            }
            else
            {
                if (address[0] != 'V')
                {
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                }

                s7AddressData.DataCode = 132;
                s7AddressData.DbBlock = 1;
                if (address.StartsWith(["VB", "VW", "VD", "VX"]))
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[2..]);
                }
                else
                {
                    s7AddressData.AddressStart = CalculateAddressStarted(address[1..]);
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
