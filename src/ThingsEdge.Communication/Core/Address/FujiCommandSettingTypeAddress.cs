using System.Text.RegularExpressions;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 富士CommandSettingsType的协议信息
/// </summary>
public class FujiCommandSettingTypeAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 数据的代号信息
    /// </summary>
    public byte DataCode { get; set; }

    /// <summary>
    /// 地址的头信息，缓存的情况
    /// </summary>
    public string AddressHeader { get; set; }

    /// <inheritdoc />
    public override void Parse(string address, ushort length)
    {
        base.Parse(address, length);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AddressHeader + AddressStart;
    }

    /// <summary>
    /// 从字符串地址解析fuji的实际地址信息，如果解析成功，则 <see cref="P:HslCommunication.OperateResult.IsSuccess" /> 为 True，取 <see cref="P:HslCommunication.OperateResult`1.Content" /> 值即可。
    /// </summary>
    /// <param name="address">字符串地址</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>是否解析成功</returns>
    public static OperateResult<FujiCommandSettingTypeAddress> ParseFrom(string address, ushort length)
    {
        try
        {
            var fujiCommandSettingTypeAddress = new FujiCommandSettingTypeAddress();
            var empty = string.Empty;
            var empty2 = string.Empty;
            if (address.IndexOf('.') < 0)
            {
                var match = Regex.Match(address, "^[A-Z]+");
                if (!match.Success)
                {
                    return new OperateResult<FujiCommandSettingTypeAddress>(StringResources.Language.NotSupportedDataType);
                }
                empty = match.Value;
                empty2 = address.Substring(empty.Length);
            }
            else
            {
                var array = address.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (array[0][0] != 'W')
                {
                    return new OperateResult<FujiCommandSettingTypeAddress>(StringResources.Language.NotSupportedDataType);
                }
                empty = array[0];
                empty2 = array[1];
            }
            fujiCommandSettingTypeAddress.AddressHeader = empty;
            fujiCommandSettingTypeAddress.AddressStart = Convert.ToInt32(empty2);
            fujiCommandSettingTypeAddress.Length = length;
            switch (empty)
            {
                case "TS":
                    fujiCommandSettingTypeAddress.DataCode = 10;
                    break;
                case "TR":
                    fujiCommandSettingTypeAddress.DataCode = 11;
                    break;
                case "CS":
                    fujiCommandSettingTypeAddress.DataCode = 12;
                    break;
                case "CR":
                    fujiCommandSettingTypeAddress.DataCode = 13;
                    break;
                case "BD":
                    fujiCommandSettingTypeAddress.DataCode = 14;
                    break;
                case "WL":
                    fujiCommandSettingTypeAddress.DataCode = 20;
                    break;
                case "B":
                    fujiCommandSettingTypeAddress.DataCode = 0;
                    break;
                case "M":
                    fujiCommandSettingTypeAddress.DataCode = 1;
                    break;
                case "K":
                    fujiCommandSettingTypeAddress.DataCode = 2;
                    break;
                case "F":
                    fujiCommandSettingTypeAddress.DataCode = 3;
                    break;
                case "A":
                    fujiCommandSettingTypeAddress.DataCode = 4;
                    break;
                case "D":
                    fujiCommandSettingTypeAddress.DataCode = 5;
                    break;
                case "S":
                    fujiCommandSettingTypeAddress.DataCode = 8;
                    break;
                default:
                    if (empty.StartsWith("W"))
                    {
                        var num = Convert.ToInt32(empty.Substring(1));
                        if (num == 9)
                        {
                            fujiCommandSettingTypeAddress.DataCode = 9;
                            break;
                        }
                        if (num >= 21 && num <= 26)
                        {
                            fujiCommandSettingTypeAddress.DataCode = (byte)num;
                            break;
                        }
                        if (num >= 30 && num <= 109)
                        {
                            fujiCommandSettingTypeAddress.DataCode = (byte)num;
                            break;
                        }
                        if (num >= 120 && num <= 123)
                        {
                            fujiCommandSettingTypeAddress.DataCode = (byte)num;
                            break;
                        }
                        if (num == 125)
                        {
                            fujiCommandSettingTypeAddress.DataCode = (byte)num;
                            break;
                        }
                        throw new Exception(StringResources.Language.NotSupportedDataType);
                    }
                    throw new Exception(StringResources.Language.NotSupportedDataType);
            }
            return OperateResult.CreateSuccessResult(fujiCommandSettingTypeAddress);
        }
        catch (Exception ex)
        {
            return new OperateResult<FujiCommandSettingTypeAddress>(GetUnsupportedAddressInfo(address, ex));
        }
    }
}
