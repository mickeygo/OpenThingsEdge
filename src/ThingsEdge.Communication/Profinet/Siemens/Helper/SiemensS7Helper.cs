using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.Siemens.Helper;

internal static class SiemensS7Helper
{
    /// <summary>
    /// 读取BOOL时，根据S7协议的返回报文，正确提取出实际的数据内容
    /// </summary>
    /// <param name="content">PLC返回的原始字节信息</param>
    /// <returns>解析之后的结果对象</returns>
    public static OperateResult<byte[]> AnalysisReadBit(byte[] content)
    {
        try
        {
            var num = 1;
            if (content.Length >= 21 && content[20] == 1)
            {
                var array = new byte[num];
                if (22 < content.Length)
                {
                    if (content[21] != byte.MaxValue || content[22] != 3)
                    {
                        if (content[21] == 5 && content[22] == 0)
                        {
                            return new OperateResult<byte[]>(content[21], StringResources.Language.SiemensReadLengthOverPlcAssign);
                        }
                        if (content[21] == 6 && content[22] == 0)
                        {
                            return new OperateResult<byte[]>(content[21], StringResources.Language.SiemensError0006);
                        }
                        if (content[21] == 10 && content[22] == 0)
                        {
                            return new OperateResult<byte[]>(content[21], StringResources.Language.SiemensError000A);
                        }
                        return new OperateResult<byte[]>(content[21], StringResources.Language.UnknownError + " Source: " + content.ToHexString(' '));
                    }
                    array[0] = content[25];
                }
                return OperateResult.CreateSuccessResult(array);
            }
            return new OperateResult<byte[]>(StringResources.Language.SiemensDataLengthCheckFailed);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("AnalysisReadBit failed: " + ex.Message + Environment.NewLine + " Msg:" + content.ToHexString(' '));
        }
    }

    public static List<S7AddressData[]> ArraySplitByLength(S7AddressData[] s7Addresses, int pduLength)
    {
        var list = new List<S7AddressData[]>();
        var list2 = new List<S7AddressData>();
        var num = 0;
        for (var i = 0; i < s7Addresses.Length; i++)
        {
            if (list2.Count >= 19 || num + s7Addresses[i].Length + list2.Count * 4 >= pduLength)
            {
                if (list2.Count > 0)
                {
                    list.Add([.. list2]);
                    list2.Clear();
                }
                num = 0;
            }
            list2.Add(s7Addresses[i]);
            num += s7Addresses[i].Length;
        }
        if (list2.Count > 0)
        {
            list.Add([.. list2]);
        }
        return list;
    }

    public static S7AddressData[] SplitS7Address(S7AddressData s7Address, int pduLength)
    {
        var list = new List<S7AddressData>();
        var i = 0;
        ushort num;
        for (int length = s7Address.Length; i < length; i += num)
        {
            num = (ushort)Math.Min(length - i, pduLength);
            var s7AddressData = new S7AddressData(s7Address);
            if (s7Address.DataCode == 31 || s7Address.DataCode == 30)
            {
                s7AddressData.AddressStart = s7Address.AddressStart + i / 2;
            }
            else
            {
                s7AddressData.AddressStart = s7Address.AddressStart + i * 8;
            }
            s7AddressData.Length = num;
            list.Add(s7AddressData);
        }
        return list.ToArray();
    }

    /// <summary>
    /// 读取字数据时，根据S7协议返回的报文，解析出实际的原始字节数组信息
    /// </summary>
    /// <param name="content">PLC返回的原始字节数组</param>
    /// <returns>实际的结果数据对象</returns>
    public static OperateResult<byte[]> AnalysisReadByte(byte[] content)
    {
        try
        {
            var list = new List<byte>();
            if (content.Length >= 21)
            {
                for (var i = 21; i < content.Length - 1; i++)
                {
                    if (content[i] == byte.MaxValue && content[i + 1] == 4)
                    {
                        var num = (content[i + 2] * 256 + content[i + 3]) / 8;
                        list.AddRange(content.SelectMiddle(i + 4, num));
                        i += num + 3;
                    }
                    else if (content[i] == byte.MaxValue && content[i + 1] == 9)
                    {
                        var num2 = content[i + 2] * 256 + content[i + 3];
                        if (num2 % 3 == 0)
                        {
                            for (var j = 0; j < num2 / 3; j++)
                            {
                                list.AddRange(content.SelectMiddle(i + 5 + 3 * j, 2));
                            }
                        }
                        else
                        {
                            for (var k = 0; k < num2 / 5; k++)
                            {
                                list.AddRange(content.SelectMiddle(i + 7 + 5 * k, 2));
                            }
                        }
                        i += num2 + 4;
                    }
                    else
                    {
                        if (content[i] == 5 && content[i + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[i], StringResources.Language.SiemensReadLengthOverPlcAssign);
                        }
                        if (content[i] == 6 && content[i + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[i], StringResources.Language.SiemensError0006);
                        }
                        if (content[i] == 10 && content[i + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[i], StringResources.Language.SiemensError000A);
                        }
                    }
                }
                return OperateResult.CreateSuccessResult(list.ToArray());
            }
            return new OperateResult<byte[]>(StringResources.Language.SiemensDataLengthCheckFailed + " Msg: " + content.ToHexString(' '));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("AnalysisReadByte failed: " + ex.Message + Environment.NewLine + " Msg:" + content.ToHexString(' '));
        }
    }

    /// <summary>
    /// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的。
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <param name="encoding">自定的编码信息，一般<see cref="Encoding.ASCII" />即可，中文需要 Encoding.GetEncoding("gb2312")</param>
    /// <returns>带有是否成功的字符串结果类对象</returns>
    public static async Task<OperateResult<string>> ReadStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, Encoding encoding)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            var read1 = await plc.ReadAsync(address, 2).ConfigureAwait(false);
            if (!read1.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read1);
            }
            if (read1.Content[0] == 0 || read1.Content[0] == byte.MaxValue)
            {
                return new OperateResult<string>("Value in plc is not string type");
            }
            var readString1 = await plc.ReadAsync(address, (ushort)(2 + read1.Content[1])).ConfigureAwait(false);
            if (!readString1.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(readString1);
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(readString1.Content, 2, readString1.Content.Length - 2));
        }

        var read2 = await plc.ReadAsync(address, 1).ConfigureAwait(false);
        if (!read2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read2);
        }
        var readString2 = await plc.ReadAsync(address, (ushort)(1 + read2.Content[0])).ConfigureAwait(false);
        if (!readString2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(readString2);
        }
        return OperateResult.CreateSuccessResult(encoding.GetString(readString2.Content, 1, readString2.Content.Length - 1));
    }

    /// <summary>
    /// 将指定的字符串写入到西门子PLC里面去，将自动添加字符串长度信息，方便PLC识别字符串的内容。
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <param name="value">写入的字符串值</param>
    /// <param name="encoding">编码信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value, Encoding encoding)
    {
        var buffer = encoding.GetBytes(value);
        if (encoding == Encoding.Unicode)
        {
            buffer = buffer.ReverseByWord();
        }
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            var readLength = await plc.ReadAsync(address, 2).ConfigureAwait(false);
            if (!readLength.IsSuccess)
            {
                return readLength;
            }
            if (readLength.Content[0] == byte.MaxValue)
            {
                return new OperateResult<string>("Value in plc is not string type");
            }
            if (readLength.Content[0] == 0)
            {
                readLength.Content[0] = 254;
            }
            if (buffer.Length > readLength.Content[0])
            {
                return new OperateResult<string>("String length is too long than plc defined");
            }
            return await plc.WriteAsync(address, CollectionUtils.SpliceArray(
            [
                readLength.Content[0],
                (byte)buffer.Length
            ], buffer)).ConfigureAwait(false);
        }
        return await plc.WriteAsync(address, CollectionUtils.SpliceArray([(byte)buffer.Length], buffer)).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的。
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <returns>带有是否成功的字符串结果类对象</returns>
    public static async Task<OperateResult<string>> ReadWStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            var read1 = await plc.ReadAsync(address, 4).ConfigureAwait(false);
            if (!read1.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read1);
            }
            var readString1 = await plc.ReadAsync(address, (ushort)(4 + (read1.Content[2] * 256 + read1.Content[3]) * 2)).ConfigureAwait(false);
            if (!readString1.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(readString1);
            }
            return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(readString1.Content.RemoveBegin(4).ReverseByWord()));
        }

        var read2 = await plc.ReadAsync(address, 1).ConfigureAwait(false);
        if (!read2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read2);
        }
        var readString2 = await plc.ReadAsync(address, (ushort)(1 + read2.Content[0] * 2)).ConfigureAwait(false);
        if (!readString2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(readString2);
        }
        return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(readString2.Content, 1, readString2.Content.Length - 1));
    }

    /// <summary>
    /// 使用双字节编码的方式，将字符串以 Unicode 编码写入到PLC的地址里，可以使用中文。
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100</param>
    /// <param name="value">字符串的值</param>
    /// <returns>是否写入成功的结果对象</returns>
    public static async Task<OperateResult> WriteWStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            var buffer = Encoding.Unicode.GetBytes(value);
            buffer = buffer.ReverseByWord();
            var readLength = await plc.ReadAsync(address, 4).ConfigureAwait(false);
            if (!readLength.IsSuccess)
            {
                return readLength;
            }

            var defineLength = readLength.Content[0] * 256 + readLength.Content[1];
            if (defineLength == 0)
            {
                defineLength = 254;
                readLength.Content[1] = 254;
            }
            if (value.Length > defineLength)
            {
                return new OperateResult<string>("String length is too long than plc defined");
            }
            var write = new byte[buffer.Length + 4];
            write[0] = readLength.Content[0];
            write[1] = readLength.Content[1];
            write[2] = BitConverter.GetBytes(value.Length)[1];
            write[3] = BitConverter.GetBytes(value.Length)[0];
            buffer.CopyTo(write, 4);
            return await plc.WriteAsync(address, write).ConfigureAwait(false);
        }
        return await plc.WriteAsync(address, value, Encoding.Unicode).ConfigureAwait(false);
    }
}
