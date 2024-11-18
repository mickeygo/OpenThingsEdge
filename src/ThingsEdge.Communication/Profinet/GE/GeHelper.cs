using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.GE;

/// <summary>
/// GE plc相关的辅助类对象
/// </summary>
public class GeHelper
{
    /// <summary>
    /// 构建一个读取数据的报文信息，需要指定操作的数据代码，读取的参数信息<br />
    /// To construct a message information for reading data, you need to specify the data code of the operation and the parameter information to be read
    /// </summary>
    /// <param name="id">消息号</param>
    /// <param name="code">操作代码</param>
    /// <param name="data">数据参数</param>
    /// <returns>包含是否成功的报文信息</returns>
    public static OperateResult<byte[]> BuildReadCoreCommand(long id, byte code, byte[] data)
    {
        var array = new byte[56]
        {
            2,
            0,
            BitConverter.GetBytes(id)[0],
            BitConverter.GetBytes(id)[1],
            0,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            6,
            192,
            0,
            0,
            0,
            0,
            16,
            14,
            0,
            0,
            1,
            1,
            code,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };
        data.CopyTo(array, 43);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 构建一个读取数据的报文命令，需要指定消息号，读取的 GE 地址信息<br />
    /// To construct a message command to read data, you need to specify the message number and read GE address information
    /// </summary>
    /// <param name="id">消息号</param>
    /// <param name="address">GE 的地址</param>
    /// <returns>包含是否成功的报文信息</returns>
    public static OperateResult<byte[]> BuildReadCommand(long id, GeSRTPAddress address)
    {
        if (address.DataCode == 10 || address.DataCode == 12 || address.DataCode == 8)
        {
            address.Length /= 2;
        }
        return BuildReadCoreCommand(id, 4, new byte[5]
        {
            address.DataCode,
            BitConverter.GetBytes(address.AddressStart)[0],
            BitConverter.GetBytes(address.AddressStart)[1],
            BitConverter.GetBytes(address.Length)[0],
            BitConverter.GetBytes(address.Length)[1]
        });
    }

    /// <summary>
    /// 构建一个读取数据的报文命令，需要指定消息号，地址，长度，是否位读取，返回完整的报文信息。<br />
    /// To construct a message command to read data, you need to specify the message number, 
    /// address, length, whether to read in bits, and return the complete message information.
    /// </summary>
    /// <param name="id">消息号</param>
    /// <param name="address">地址</param>
    /// <param name="length">读取的长度</param>
    /// <param name="isBit"></param>
    /// <returns>包含是否成功的报文对象</returns>
    public static OperateResult<byte[]> BuildReadCommand(long id, string address, ushort length, bool isBit)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, length, isBit);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return BuildReadCommand(id, operateResult.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.GE.GeHelper.BuildWriteCommand(System.Int64,System.String,System.Byte[])" />
    public static OperateResult<byte[]> BuildWriteCommand(long id, GeSRTPAddress address, byte[] value)
    {
        int num = address.Length;
        if (address.DataCode == 10 || address.DataCode == 12 || address.DataCode == 8)
        {
            num /= 2;
        }
        var array = new byte[56 + value.Length];
        array[0] = 2;
        array[1] = 0;
        array[2] = BitConverter.GetBytes(id)[0];
        array[3] = BitConverter.GetBytes(id)[1];
        array[4] = BitConverter.GetBytes(value.Length)[0];
        array[5] = BitConverter.GetBytes(value.Length)[1];
        array[9] = 2;
        array[17] = 2;
        array[18] = 0;
        array[30] = 9;
        array[31] = 128;
        array[36] = 16;
        array[37] = 14;
        array[40] = 1;
        array[41] = 1;
        array[42] = 2;
        array[48] = 1;
        array[49] = 1;
        array[50] = 7;
        array[51] = address.DataCode;
        array[52] = BitConverter.GetBytes(address.AddressStart)[0];
        array[53] = BitConverter.GetBytes(address.AddressStart)[1];
        array[54] = BitConverter.GetBytes(num)[0];
        array[55] = BitConverter.GetBytes(num)[1];
        value.CopyTo(array, 56);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 构建一个批量写入 byte 数组变量的报文，需要指定消息号，写入的地址，地址参照 <see cref="T:HslCommunication.Profinet.GE.GeSRTPNet" /> 说明。<br />
    /// To construct a message to be written into byte array variables in batches, 
    /// you need to specify the message number and write address. For the address, refer to the description of <see cref="T:HslCommunication.Profinet.GE.GeSRTPNet" />.
    /// </summary>
    /// <param name="id">消息的序号</param>
    /// <param name="address">地址信息</param>
    /// <param name="value">byte数组的原始数据</param>
    /// <returns>包含结果信息的报文内容</returns>
    public static OperateResult<byte[]> BuildWriteCommand(long id, string address, byte[] value)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, (ushort)value.Length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return BuildWriteCommand(id, operateResult.Content, value);
    }

    /// <summary>
    /// 构建一个批量写入 bool 数组变量的报文，需要指定消息号，写入的地址，地址参照 <see cref="T:HslCommunication.Profinet.GE.GeSRTPNet" /> 说明。<br />
    /// To construct a message to be written into bool array variables in batches, 
    /// you need to specify the message number and write address. For the address, refer to the description of <see cref="T:HslCommunication.Profinet.GE.GeSRTPNet" />.
    /// </summary>
    /// <param name="id">消息的序号</param>
    /// <param name="address">地址信息</param>
    /// <param name="value">bool数组</param>
    /// <returns>包含结果信息的报文内容</returns>
    public static OperateResult<byte[]> BuildWriteCommand(long id, string address, bool[] value)
    {
        var operateResult = GeSRTPAddress.ParseFrom(address, (ushort)value.Length, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var array = new bool[operateResult.Content.AddressStart % 8 + value.Length];
        value.CopyTo(array, operateResult.Content.AddressStart % 8);
        return BuildWriteCommand(id, operateResult.Content, SoftBasic.BoolArrayToByte(array));
    }

    /// <summary>
    /// 从PLC返回的数据中，提取出实际的数据内容，最少6个字节的数据。超出实际的数据长度的部分没有任何意义。<br />
    /// From the data returned by the PLC, extract the actual data content, at least 6 bytes of data. The part beyond the actual data length has no meaning.
    /// </summary>
    /// <param name="content">PLC返回的数据信息</param>
    /// <returns>解析后的实际数据内容</returns>
    public static OperateResult<byte[]> ExtraResponseContent(byte[] content)
    {
        try
        {
            if (content[0] != 3)
            {
                return new OperateResult<byte[]>(content[0], StringResources.Language.UnknownError + " Source:" + content.ToHexString(' '));
            }
            if (content[31] == 212)
            {
                var num = BitConverter.ToUInt16(content, 42);
                if (num != 0)
                {
                    return new OperateResult<byte[]>(num, StringResources.Language.UnknownError);
                }
                return OperateResult.CreateSuccessResult(content.SelectMiddle(44, 6));
            }
            if (content[31] == 148)
            {
                return OperateResult.CreateSuccessResult(content.RemoveBegin(56));
            }
            return new OperateResult<byte[]>("Extra Wrong:" + StringResources.Language.UnknownError + " Source:" + content.ToHexString(' '));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Extra Wrong:" + ex.Message + " Source:" + content.ToHexString(' '));
        }
    }

    /// <summary>
    /// 从实际的时间的字节数组里解析出C#格式的时间对象，这个时间可能是时区0的时间，需要自行转化本地时间。<br />
    /// Analyze the time object in C# format from the actual time byte array. 
    /// This time may be the time in time zone 0, and you need to convert the local time yourself.
    /// </summary>
    /// <param name="content">字节数组</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<DateTime> ExtraDateTime(byte[] content)
    {
        try
        {
            return OperateResult.CreateSuccessResult(new DateTime(int.Parse(content[5].ToString("X2")) + 2000, int.Parse(content[4].ToString("X2")), int.Parse(content[3].ToString("X2")), int.Parse(content[2].ToString("X2")), int.Parse(content[1].ToString("X2")), int.Parse(content[0].ToString("X2"))));
        }
        catch (Exception ex)
        {
            return new OperateResult<DateTime>(ex.Message + " Source:" + content.ToHexString(' '));
        }
    }

    /// <summary>
    /// 从实际的时间的字节数组里解析出PLC的程序的名称。<br />
    /// Parse the name of the PLC program from the actual time byte array
    /// </summary>
    /// <param name="content">字节数组</param>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<string> ExtraProgramName(byte[] content)
    {
        try
        {
            return OperateResult.CreateSuccessResult(Encoding.UTF8.GetString(content, 18, 16).Trim(default(char)));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message + " Source:" + content.ToHexString(' '));
        }
    }
}
