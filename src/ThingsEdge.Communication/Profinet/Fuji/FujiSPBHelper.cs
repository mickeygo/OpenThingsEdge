using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士SPB的辅助类
/// </summary>
public class FujiSPBHelper
{
    /// <summary>
    /// 将int数据转换成SPB可识别的标准的数据内容，例如 2转换为0200 , 200转换为0002
    /// </summary>
    /// <param name="address">等待转换的数据内容</param>
    /// <returns>转换之后的数据内容</returns>
    public static string AnalysisIntegerAddress(int address)
    {
        var text = address.ToString("D4");
        return text.Substring(2) + text.Substring(0, 2);
    }

    /// <summary>
    /// 计算指令的和校验码
    /// </summary>
    /// <param name="data">指令</param>
    /// <returns>校验之后的信息</returns>
    public static string CalculateAcc(string data)
    {
        var bytes = Encoding.ASCII.GetBytes(data);
        var num = 0;
        for (var i = 0; i < bytes.Length; i++)
        {
            num += bytes[i];
        }
        return num.ToString("X4").Substring(2);
    }

    /// <summary>
    /// 创建一条读取的指令信息，需要指定一些参数，单次读取最大105个字
    /// </summary>
    /// <param name="station">PLC的站号</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = FujiSPBAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return BuildReadCommand(station, operateResult.Content, length);
    }

    /// <summary>
    /// 创建一条读取的指令信息，需要指定一些参数，单次读取最大105个字
    /// </summary>
    /// <param name="station">PLC的站号</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildReadCommand(byte station, FujiSPBAddress address, ushort length)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(':');
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("09");
        stringBuilder.Append("FFFF");
        stringBuilder.Append("00");
        stringBuilder.Append("00");
        stringBuilder.Append(address.GetWordAddress());
        stringBuilder.Append(AnalysisIntegerAddress(length));
        stringBuilder.Append("\r\n");
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 创建一条读取多个地址的指令信息，需要指定一些参数，单次读取最大105个字
    /// </summary>
    /// <param name="station">PLC的站号</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <param name="isBool">是否位读取</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> BuildReadCommand(byte station, string[] address, ushort[] length, bool isBool)
    {
        if (address == null || length == null)
        {
            return new OperateResult<byte[]>("Parameter address or length can't be null");
        }
        if (address.Length != length.Length)
        {
            return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(':');
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append((6 + address.Length * 4).ToString("X2"));
        stringBuilder.Append("FFFF");
        stringBuilder.Append("00");
        stringBuilder.Append("04");
        stringBuilder.Append("00");
        stringBuilder.Append(address.Length.ToString("X2"));
        for (var i = 0; i < address.Length; i++)
        {
            station = (byte)CommHelper.ExtractParameter(ref address[i], "s", station);
            var operateResult = FujiSPBAddress.ParseFrom(address[i]);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }
            stringBuilder.Append(operateResult.Content.TypeCode);
            stringBuilder.Append(length[i].ToString("X2"));
            stringBuilder.Append(AnalysisIntegerAddress(operateResult.Content.AddressStart));
        }
        stringBuilder[1] = station.ToString("X2")[0];
        stringBuilder[2] = station.ToString("X2")[1];
        stringBuilder.Append("\r\n");
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位，单次写入最大103个字
    /// </summary>
    /// <param name="station">站号</param>
    /// <param name="address">地址</param>
    /// <param name="value">数组值</param>
    /// <returns>是否创建成功</returns>
    public static OperateResult<byte[]> BuildWriteByteCommand(byte station, string address, byte[] value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = FujiSPBAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(':');
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("00");
        stringBuilder.Append("FFFF");
        stringBuilder.Append("01");
        stringBuilder.Append("00");
        stringBuilder.Append(operateResult.Content.GetWordAddress());
        stringBuilder.Append(AnalysisIntegerAddress(value.Length / 2));
        stringBuilder.Append(value.ToHexString());
        stringBuilder[3] = ((stringBuilder.Length - 5) / 2).ToString("X2")[0];
        stringBuilder[4] = ((stringBuilder.Length - 5) / 2).ToString("X2")[1];
        stringBuilder.Append("\r\n");
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位，单次写入最大103个字
    /// </summary>
    /// <param name="station">站号</param>
    /// <param name="address">地址</param>
    /// <param name="value">数组值</param>
    /// <returns>是否创建成功</returns>
    public static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool value)
    {
        station = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = FujiSPBAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
        {
            operateResult.Content.BitIndex = operateResult.Content.AddressStart % 16;
            operateResult.Content.AddressStart = (ushort)(operateResult.Content.AddressStart / 16);
        }
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(':');
        stringBuilder.Append(station.ToString("X2"));
        stringBuilder.Append("00");
        stringBuilder.Append("FFFF");
        stringBuilder.Append("01");
        stringBuilder.Append("02");
        stringBuilder.Append(operateResult.Content.GetWriteBoolAddress());
        stringBuilder.Append(value ? "01" : "00");
        stringBuilder[3] = ((stringBuilder.Length - 5) / 2).ToString("X2")[0];
        stringBuilder[4] = ((stringBuilder.Length - 5) / 2).ToString("X2")[1];
        stringBuilder.Append("\r\n");
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
    }

    /// <summary>
    /// 检查反馈的数据信息，是否包含了错误码，如果没有包含，则返回成功
    /// </summary>
    /// <param name="content">原始的报文返回</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> CheckResponseData(byte[] content)
    {
        try
        {
            if (content[0] != 58)
            {
                return new OperateResult<byte[]>(content[0], "Read Faild:" + SoftBasic.ByteToHexString(content, ' '));
            }
            var @string = Encoding.ASCII.GetString(content, 9, 2);
            if (@string != "00")
            {
                return new OperateResult<byte[]>(Convert.ToInt32(@string, 16), GetErrorDescriptionFromCode(@string));
            }
            if (content[content.Length - 2] == 13 && content[content.Length - 1] == 10)
            {
                content = content.RemoveLast(2);
            }
            return OperateResult.CreateSuccessResult(content.RemoveBegin(11));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("CheckResponseData failed: " + ex.Message + Environment.NewLine + "Source: " + content.ToHexString(' '));
        }
    }

    /// <summary>
    /// 根据错误码获取到真实的文本信息
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>错误的文本描述</returns>
    public static string GetErrorDescriptionFromCode(string code)
    {
        return code switch
        {
            "01" => StringResources.Language.FujiSpbStatus01,
            "02" => StringResources.Language.FujiSpbStatus02,
            "03" => StringResources.Language.FujiSpbStatus03,
            "04" => StringResources.Language.FujiSpbStatus04,
            "05" => StringResources.Language.FujiSpbStatus05,
            "06" => StringResources.Language.FujiSpbStatus06,
            "07" => StringResources.Language.FujiSpbStatus07,
            "09" => StringResources.Language.FujiSpbStatus09,
            "0C" => StringResources.Language.FujiSpbStatus0C,
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 批量读取PLC的数据，以字为单位，支持读取X,Y,L,M,D,TN,CN,TC,CC,R,W具体的地址范围需要根据PLC型号来确认，地址可以携带站号信息，例如：s=2;D100<br />
    /// Read PLC data in batches, in units of words. Supports reading X, Y, L, M, D, TN, CN, TC, CC, R, W. 
    /// The specific address range needs to be confirmed according to the PLC model, The address can carry station number information, for example: s=2;D100
    /// </summary>
    /// <param name="device">PLC设备通信对象</param>
    /// <param name="station">当前的站号信息</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>读取结果信息</returns>
    /// <remarks>
    /// 单次读取的最大的字数为105，如果读取的字数超过这个值，请分批次读取。
    /// </remarks>
    public static OperateResult<byte[]> Read(IReadWriteDevice device, byte station, string address, ushort length)
    {
        var operateResult = BuildReadCommand(station, address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        OperateResult<byte[]> operateResult2 = device.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        var operateResult3 = CheckResponseData(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult3.Content.RemoveBegin(4)).ToHexBytes());
    }

    /// <summary>
    /// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持读取X,Y,L,M,D,TN,CN,TC,CC,R具体的地址范围需要根据PLC型号来确认，地址可以携带站号信息，例如：s=2;D100<br />
    /// The data written to the PLC in batches, in units of words, that is, a minimum of 2 bytes of information. It supports reading X, Y, L, M, D, TN, CN, TC, CC, and R. 
    /// The specific address range needs to be based on PLC model to confirm, The address can carry station number information, for example: s=2;D100
    /// </summary>
    /// <param name="device">PLC设备通信对象</param>
    /// <param name="station">当前的站号信息</param>
    /// <param name="address">地址信息，举例，D100，R200，TN100，CN200</param>
    /// <param name="value">数据值</param>
    /// <returns>是否写入成功</returns>
    /// <remarks>
    /// 单次写入的最大的字数为103个字，如果写入的数据超过这个长度，请分批次写入
    /// </remarks>
    public static OperateResult Write(IReadWriteDevice device, byte station, string address, byte[] value)
    {
        var operateResult = BuildWriteByteCommand(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = device.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return CheckResponseData(operateResult2.Content);
    }

    /// <summary>
    /// 批量读取PLC的Bool数据，以位为单位，支持读取X,Y,L,M,D,TN,CN,TC,CC,R,W，例如 M100, 如果是寄存器地址，可以使用D10.12来访问第10个字的12位，地址可以携带站号信息，例如：s=2;M100<br />
    /// Read PLC's Bool data in batches, in units of bits, support reading X, Y, L, M, D, TN, CN, TC, CC, R, W, such as M100, if it is a register address, 
    /// you can use D10. 12 to access the 12 bits of the 10th word, the address can carry station number information, for example: s=2;M100
    /// </summary>
    /// <param name="device">PLC设备通信对象</param>
    /// <param name="station">当前的站号信息</param>
    /// <param name="address">地址信息，举例：M100, D10.12</param>
    /// <param name="length">读取的bool长度信息</param>
    /// <returns>Bool[]的结果对象</returns>
    public static OperateResult<bool[]> ReadBool(IReadWriteDevice device, byte station, string address, ushort length)
    {
        var station2 = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var operateResult = FujiSPBAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
        {
            operateResult.Content.BitIndex = operateResult.Content.AddressStart % 16;
            operateResult.Content.AddressStart = (ushort)(operateResult.Content.AddressStart / 16);
        }
        var length2 = (ushort)((operateResult.Content.GetBitIndex() + length - 1) / 16 - operateResult.Content.GetBitIndex() / 16 + 1);
        var operateResult2 = BuildReadCommand(station2, operateResult.Content, length2);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        OperateResult<byte[]> operateResult3 = device.ReadFromCoreServer(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult3);
        }
        var operateResult4 = CheckResponseData(operateResult3.Content);
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult4);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult4.Content.RemoveBegin(4)).ToHexBytes().ToBoolArray()
            .SelectMiddle(operateResult.Content.BitIndex, length));
    }

    /// <summary>
    /// 写入一个Bool值到一个地址里，地址可以是线圈地址，也可以是寄存器地址，例如：M100, D10.12，地址可以携带站号信息，例如：s=2;D10.12<br />
    /// Write a Bool value to an address. The address can be a coil address or a register address, for example: M100, D10.12. 
    /// The address can carry station number information, for example: s=2;D10.12
    /// </summary>
    /// <param name="device">PLC设备通信对象</param>
    /// <param name="station">当前的站号信息</param>
    /// <param name="address">地址信息，举例：M100, D10.12</param>
    /// <param name="value">写入的bool值</param>
    /// <returns>是否写入成功的结果对象</returns>
    public static OperateResult Write(IReadWriteDevice device, byte station, string address, bool value)
    {
        var operateResult = BuildWriteBoolCommand(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = device.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return CheckResponseData(operateResult2.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Read(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteDevice device, byte station, string address, ushort length)
    {
        var command = BuildReadCommand(station, address, length);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var read = await device.ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        var check = CheckResponseData(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(check.Content.RemoveBegin(4)).ToHexBytes());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Byte[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice device, byte station, string address, byte[] value)
    {
        var command = BuildWriteByteCommand(station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await device.ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseData(read.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.UInt16)" />
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteDevice device, byte station, string address, ushort length)
    {
        var stat = (byte)CommHelper.ExtractParameter(ref address, "s", station);
        var addressAnalysis = FujiSPBAddress.ParseFrom(address);
        if (!addressAnalysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(addressAnalysis);
        }
        if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
        {
            addressAnalysis.Content.BitIndex = addressAnalysis.Content.AddressStart % 16;
            addressAnalysis.Content.AddressStart = (ushort)(addressAnalysis.Content.AddressStart / 16);
        }
        var command = BuildReadCommand(length: (ushort)((addressAnalysis.Content.GetBitIndex() + length - 1) / 16 - addressAnalysis.Content.GetBitIndex() / 16 + 1), station: stat, address: addressAnalysis.Content);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var read = await device.ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        var check = CheckResponseData(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(check);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(check.Content.RemoveBegin(4)).ToHexBytes().ToBoolArray()
            .SelectMiddle(addressAnalysis.Content.BitIndex, length));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Fuji.FujiSPBHelper.Write(HslCommunication.Core.IReadWriteDevice,System.Byte,System.String,System.Boolean)" />
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice device, byte station, string address, bool value)
    {
        var command = BuildWriteBoolCommand(station, address, value);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await device.ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseData(read.Content);
    }
}
