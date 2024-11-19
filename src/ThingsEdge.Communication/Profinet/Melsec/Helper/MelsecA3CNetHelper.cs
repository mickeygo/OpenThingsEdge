using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// MelsecA3CNet 协议通信的辅助类。
/// </summary>
public static class MelsecA3CNetHelper
{
    /// <summary>
    /// 将命令进行打包传送，可选站号及是否和校验机制。
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <param name="mcCommand">mc协议的命令</param>
    /// <param name="station">PLC的站号</param>
    /// <returns>最终的原始报文信息</returns>
    public static byte[] PackCommand(IReadWriteA3C plc, byte[] mcCommand, byte station = 0)
    {
        var memoryStream = new MemoryStream();
        if (plc.Format != 3)
        {
            memoryStream.WriteByte(5);
        }
        else
        {
            memoryStream.WriteByte(2);
        }
        if (plc.Format == 2)
        {
            memoryStream.WriteByte(48);
            memoryStream.WriteByte(48);
        }
        memoryStream.WriteByte(70);
        memoryStream.WriteByte(57);
        memoryStream.WriteByte(SoftBasic.BuildAsciiBytesFrom(station)[0]);
        memoryStream.WriteByte(SoftBasic.BuildAsciiBytesFrom(station)[1]);
        memoryStream.WriteByte(48);
        memoryStream.WriteByte(48);
        memoryStream.WriteByte(70);
        memoryStream.WriteByte(70);
        memoryStream.WriteByte(48);
        memoryStream.WriteByte(48);
        memoryStream.Write(mcCommand, 0, mcCommand.Length);
        if (plc.Format == 3)
        {
            memoryStream.WriteByte(3);
        }
        if (plc.SumCheck)
        {
            var array = memoryStream.ToArray();
            var num = 0;
            for (var i = 1; i < array.Length; i++)
            {
                num += array[i];
            }
            memoryStream.WriteByte(SoftBasic.BuildAsciiBytesFrom((byte)num)[0]);
            memoryStream.WriteByte(SoftBasic.BuildAsciiBytesFrom((byte)num)[1]);
        }
        if (plc.Format == 4)
        {
            memoryStream.WriteByte(13);
            memoryStream.WriteByte(10);
        }
        var result = memoryStream.ToArray();
        memoryStream.Dispose();
        return result;
    }

    private static int GetErrorCodeOrDataStartIndex(IReadWriteA3C plc)
    {
        var result = 11;
        switch (plc.Format)
        {
            case 1:
                result = 11;
                break;
            case 2:
                result = 13;
                break;
            case 3:
                result = 15;
                break;
            case 4:
                result = 11;
                break;
        }
        return result;
    }

    /// <summary>
    /// 根据PLC返回的数据信息，获取到实际的数据内容
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <param name="response">PLC返回的数据信息</param>
    /// <returns>带有是否成功的读取结果对象内容</returns>
    private static OperateResult<byte[]> ExtraReadActualResponse(IReadWriteA3C plc, byte[] response)
    {
        try
        {
            var errorCodeOrDataStartIndex = GetErrorCodeOrDataStartIndex(plc);
            if (plc.Format == 1 || plc.Format == 2 || plc.Format == 4)
            {
                if (response[0] == 21)
                {
                    var num = Convert.ToInt32(Encoding.ASCII.GetString(response, errorCodeOrDataStartIndex, 4), 16);
                    return new OperateResult<byte[]>(num, MelsecHelper.GetErrorDescription(num));
                }
                if (response[0] != 2)
                {
                    return new OperateResult<byte[]>(response[0], "Read Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
            }
            else if (plc.Format == 3)
            {
                var @string = Encoding.ASCII.GetString(response, 11, 4);
                if (@string == "QNAK")
                {
                    var num2 = Convert.ToInt32(Encoding.ASCII.GetString(response, errorCodeOrDataStartIndex, 4), 16);
                    return new OperateResult<byte[]>(num2, MelsecHelper.GetErrorDescription(num2));
                }
                if (@string != "QACK")
                {
                    return new OperateResult<byte[]>(response[0], "Read Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
            }
            var num3 = -1;
            for (var i = errorCodeOrDataStartIndex; i < response.Length; i++)
            {
                if (response[i] == 3)
                {
                    num3 = i;
                    break;
                }
            }
            if (num3 == -1)
            {
                num3 = response.Length;
            }
            return OperateResult.CreateSuccessResult(response.SelectMiddle(errorCodeOrDataStartIndex, num3 - errorCodeOrDataStartIndex));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("ExtraReadActualResponse Wrong:" + ex.Message + Environment.NewLine + "Source: " + response.ToHexString(' '));
        }
    }

    private static OperateResult CheckWriteResponse(IReadWriteA3C plc, byte[] response)
    {
        var errorCodeOrDataStartIndex = GetErrorCodeOrDataStartIndex(plc);
        try
        {
            if (plc.Format == 1 || plc.Format == 2)
            {
                if (response[0] == 21)
                {
                    var num = Convert.ToInt32(Encoding.ASCII.GetString(response, errorCodeOrDataStartIndex, 4), 16);
                    return new OperateResult<byte[]>(num, MelsecHelper.GetErrorDescription(num));
                }
                if (response[0] != 6)
                {
                    return new OperateResult<byte[]>(response[0], "Write Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
            }
            else if (plc.Format == 3)
            {
                if (response[0] != 2)
                {
                    return new OperateResult<byte[]>(response[0], "Write Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
                var @string = Encoding.ASCII.GetString(response, 11, 4);
                if (@string == "QNAK")
                {
                    var num2 = Convert.ToInt32(Encoding.ASCII.GetString(response, errorCodeOrDataStartIndex, 4), 16);
                    return new OperateResult<byte[]>(num2, MelsecHelper.GetErrorDescription(num2));
                }
                if (@string != "QACK")
                {
                    return new OperateResult<byte[]>(response[0], "Write Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
            }
            else if (plc.Format == 4)
            {
                if (response[0] == 21)
                {
                    var num3 = Convert.ToInt32(Encoding.ASCII.GetString(response, errorCodeOrDataStartIndex, 4), 16);
                    return new OperateResult<byte[]>(num3, MelsecHelper.GetErrorDescription(num3));
                }
                if (response[0] != 6)
                {
                    return new OperateResult<byte[]>(response[0], "Write Faild:" + SoftBasic.GetAsciiStringRender(response));
                }
            }
            return OperateResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("CheckWriteResponse failed: " + ex.Message + Environment.NewLine + "Content: " + SoftBasic.GetAsciiStringRender(response));
        }
    }

    /// <summary>
    /// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认。
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>读取结果信息</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteA3C plc, string address, ushort length)
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var addressResult = McAddressData.ParseMelsecFrom(address, length, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }

        var bytesContent = new List<byte>();
        ushort alreadyFinished = 0;
        while (alreadyFinished < length)
        {
            var readLength = (ushort)Math.Min(length - alreadyFinished, McHelper.GetReadWordLength(McType.MCAscii));
            addressResult.Content.Length = readLength;
            var command = McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: false);
            var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, command, stat)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var check = ExtraReadActualResponse(plc, read.Content);
            if (!check.IsSuccess)
            {
                return check;
            }
            bytesContent.AddRange(MelsecHelper.TransAsciiByteArrayToByteArray(check.Content));
            alreadyFinished += readLength;
            if (addressResult.Content.McDataType.DataType == 0)
            {
                addressResult.Content.AddressStart += readLength;
            }
            else
            {
                addressResult.Content.AddressStart += readLength * 16;
            }
        }
        return OperateResult.CreateSuccessResult(bytesContent.ToArray());
    }

    /// <summary>
    /// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认。
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <param name="address">地址信息</param>
    /// <param name="value">数据值</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteA3C plc, string address, byte[] value)
    {
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var addressResult = McAddressData.ParseMelsecFrom(address, 0, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        var command = McAsciiHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value);
        var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, command, stat)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckWriteResponse(plc, read.Content);
    }

    /// <summary>
    /// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
    /// <param name="length">读取的长度</param>
    /// <returns>读取结果信息</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteA3C plc, string address, ushort length)
    {
        if (address.IndexOf('.') > 0)
        {
            return await CommunicationHelper.ReadBoolAsync(plc, address, length).ConfigureAwait(false);
        }
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var addressResult = McAddressData.ParseMelsecFrom(address, length, isBit: true);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(addressResult);
        }

        var boolContent = new List<bool>();
        ushort alreadyFinished = 0;
        while (alreadyFinished < length)
        {
            var readLength = (ushort)Math.Min(length - alreadyFinished, McHelper.GetReadBoolLength(McType.MCAscii));
            addressResult.Content.Length = readLength;
            var command = McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: true);
            var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, command, stat)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            var check = ExtraReadActualResponse(plc, read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(check);
            }
            boolContent.AddRange(check.Content.Select((m) => m == 49).ToArray());
            alreadyFinished += readLength;
            addressResult.Content.AddressStart += readLength;
        }
        return OperateResult.CreateSuccessResult(boolContent.ToArray());
    }

    public static async Task<OperateResult> WriteAsync(IReadWriteA3C plc, string address, bool[] value)
    {
        if (plc.EnableWriteBitToWordRegister && address.Contains('.'))
        {
            return await ReadWriteNetHelper.WriteBoolWithWordAsync(plc, address, value).ConfigureAwait(continueOnCapturedContext: false);
        }
        var stat = (byte)CommunicationHelper.ExtractParameter(ref address, "s", plc.Station);
        var addressResult = McAddressData.ParseMelsecFrom(address, 0, isBit: true);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(addressResult);
        }
        var command = McAsciiHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, value);
        var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, command, stat)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckWriteResponse(plc, read.Content);
    }

    /// <summary>
    /// 远程Run操作
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> RemoteRunAsync(IReadWriteA3C plc)
    {
        var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, Encoding.ASCII.GetBytes("1001000000010000"), plc.Station)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckWriteResponse(plc, read.Content);
    }

    /// <summary>
    /// 远程Stop操作
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> RemoteStopAsync(IReadWriteA3C plc)
    {
        var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, Encoding.ASCII.GetBytes("100200000001"), plc.Station)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckWriteResponse(plc, read.Content);
    }

    /// <summary>
    /// 读取PLC的型号信息
    /// </summary>
    /// <param name="plc">PLC设备通信对象</param>
    /// <returns>返回型号的结果对象</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteA3C plc)
    {
        var read = await plc.ReadFromCoreServerAsync(PackCommand(plc, Encoding.ASCII.GetBytes("01010000"), plc.Station)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        var check = ExtraReadActualResponse(plc, read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(check);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(check.Content, 0, 16).TrimEnd());
    }
}
