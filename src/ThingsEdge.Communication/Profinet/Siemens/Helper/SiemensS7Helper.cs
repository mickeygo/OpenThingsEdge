using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Siemens.Helper;

internal class SiemensS7Helper
{
    /// <summary>
    /// 读取BOOL时，根据S7协议的返回报文，正确提取出实际的数据内容
    /// </summary>
    /// <param name="content">PLC返回的原始字节信息</param>
    /// <returns>解析之后的结果对象</returns>
    internal static OperateResult<byte[]> AnalysisReadBit(byte[] content)
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
            return new OperateResult<byte[]>("AnalysisReadBit failed: " + ex.Message + Environment.NewLine + " Msg:" + SoftBasic.ByteToHexString(content, ' '));
        }
    }

    internal static List<S7AddressData[]> ArraySplitByLength(S7AddressData[] s7Addresses, int pduLength)
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
                    list.Add(list2.ToArray());
                    list2.Clear();
                }
                num = 0;
            }
            list2.Add(s7Addresses[i]);
            num += s7Addresses[i].Length;
        }
        if (list2.Count > 0)
        {
            list.Add(list2.ToArray());
        }
        return list;
    }

    internal static S7AddressData[] SplitS7Address(S7AddressData s7Address, int pduLength)
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
    internal static OperateResult<byte[]> AnalysisReadByte(byte[] content)
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
            return new OperateResult<byte[]>(StringResources.Language.SiemensDataLengthCheckFailed + " Msg: " + SoftBasic.ByteToHexString(content, ' '));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("AnalysisReadByte failed: " + ex.Message + Environment.NewLine + " Msg:" + SoftBasic.ByteToHexString(content, ' '));
        }
    }

    /// <summary>
    /// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的<br />
    /// Read the Siemens address string information. This information is bound to Siemens and its length changes dynamically with the Siemens information
    /// </summary>
    /// <remarks>
    /// 如果指定编码，一般<see cref="P:System.Text.Encoding.ASCII" />即可，中文需要 Encoding.GetEncoding("gb2312")
    /// </remarks>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <param name="encoding">自定的编码信息，一般<see cref="P:System.Text.Encoding.ASCII" />即可，中文需要 Encoding.GetEncoding("gb2312")</param>
    /// <returns>带有是否成功的字符串结果类对象</returns>
    public static OperateResult<string> ReadString(IReadWriteNet plc, SiemensPLCS currentPlc, string address, Encoding encoding)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            OperateResult<byte[]> operateResult = plc.Read(address, 2);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(operateResult);
            }
            if (operateResult.Content[0] == 0 || operateResult.Content[0] == byte.MaxValue)
            {
                return new OperateResult<string>("Value in plc is not string type");
            }
            OperateResult<byte[]> operateResult2 = plc.Read(address, (ushort)(2 + operateResult.Content[1]));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(operateResult2);
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(operateResult2.Content, 2, operateResult2.Content.Length - 2));
        }
        OperateResult<byte[]> operateResult3 = plc.Read(address, 1);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult3);
        }
        OperateResult<byte[]> operateResult4 = plc.Read(address, (ushort)(1 + operateResult3.Content[0]));
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult4);
        }
        return OperateResult.CreateSuccessResult(encoding.GetString(operateResult4.Content, 1, operateResult4.Content.Length - 1));
    }

    /// <summary>
    /// 将指定的字符串写入到西门子PLC里面去，将自动添加字符串长度信息，方便PLC识别字符串的内容。<br />
    /// Write the specified string into Siemens PLC, and the string length information will be automatically added, which is convenient for PLC to identify the content of the string.
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <param name="value">写入的字符串值</param>
    /// <param name="encoding">编码信息</param>
    /// <returns>是否写入成功</returns>
    public static OperateResult Write(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value, Encoding encoding)
    {
        if (value == null)
        {
            value = string.Empty;
        }
        var array = encoding.GetBytes(value);
        if (encoding == Encoding.Unicode)
        {
            array = SoftBasic.BytesReverseByWord(array);
        }
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            OperateResult<byte[]> operateResult = plc.Read(address, 2);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            if (operateResult.Content[0] == byte.MaxValue)
            {
                return new OperateResult<string>("Value in plc is not string type");
            }
            if (operateResult.Content[0] == 0)
            {
                operateResult.Content[0] = 254;
            }
            if (array.Length > operateResult.Content[0])
            {
                return new OperateResult<string>("String length is too long than plc defined");
            }
            return plc.Write(address, SoftBasic.SpliceArray(new byte[2]
            {
                operateResult.Content[0],
                (byte)array.Length
            }, array));
        }
        return plc.Write(address, SoftBasic.SpliceArray(new byte[1] { (byte)array.Length }, array));
    }

    /// <summary>
    /// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的<br />
    /// Read the Siemens address string information. This information is bound to Siemens and its length changes dynamically with the Siemens information
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
    /// <returns>带有是否成功的字符串结果类对象</returns>
    public static OperateResult<string> ReadWString(IReadWriteNet plc, SiemensPLCS currentPlc, string address)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            OperateResult<byte[]> operateResult = plc.Read(address, 4);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(operateResult);
            }
            OperateResult<byte[]> operateResult2 = plc.Read(address, (ushort)(4 + (operateResult.Content[2] * 256 + operateResult.Content[3]) * 2));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(operateResult2);
            }
            return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(SoftBasic.BytesReverseByWord(operateResult2.Content.RemoveBegin(4))));
        }
        OperateResult<byte[]> operateResult3 = plc.Read(address, 1);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult3);
        }
        OperateResult<byte[]> operateResult4 = plc.Read(address, (ushort)(1 + operateResult3.Content[0] * 2));
        if (!operateResult4.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult4);
        }
        return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(operateResult4.Content, 1, operateResult4.Content.Length - 1));
    }

    /// <summary>
    /// 使用双字节编码的方式，将字符串以 Unicode 编码写入到PLC的地址里，可以使用中文。<br />
    /// Use the double-byte encoding method to write the character string to the address of the PLC in Unicode encoding. Chinese can be used.
    /// </summary>
    /// <param name="plc">PLC的通信对象</param>
    /// <param name="currentPlc">PLC的系列信息</param>
    /// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100 -&gt; Starting address, formatted as I100,mM100,Q100,DB20.100</param>
    /// <param name="value">字符串的值</param>
    /// <returns>是否写入成功的结果对象</returns>
    public static OperateResult WriteWString(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            var array = Encoding.Unicode.GetBytes(value).ReverseByWord();
            OperateResult<byte[]> operateResult = plc.Read(address, 4);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            var num = operateResult.Content[0] * 256 + operateResult.Content[1];
            if (num == 0)
            {
                num = 254;
                operateResult.Content[1] = 254;
            }
            if (value.Length > num)
            {
                return new OperateResult<string>("String length is too long than plc defined");
            }
            var array2 = new byte[array.Length + 4];
            array2[0] = operateResult.Content[0];
            array2[1] = operateResult.Content[1];
            array2[2] = BitConverter.GetBytes(value.Length)[1];
            array2[3] = BitConverter.GetBytes(value.Length)[0];
            array.CopyTo(array2, 4);
            return plc.Write(address, array2);
        }
        return plc.Write(address, value, Encoding.Unicode);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensS7Helper.ReadString(HslCommunication.Core.IReadWriteNet,HslCommunication.Profinet.Siemens.SiemensPLCS,System.String,System.Text.Encoding)" />
    public static async Task<OperateResult<string>> ReadStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, Encoding encoding)
    {
        OperateResult<byte[]> read;
        OperateResult<byte[]> readString;
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            read = await plc.ReadAsync(address, 2);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read);
            }
            if (read.Content[0] == 0 || read.Content[0] == byte.MaxValue)
            {
                return new OperateResult<string>("Value in plc is not string type");
            }
            readString = await plc.ReadAsync(address, (ushort)(2 + read.Content[1]));
            if (!readString.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(readString);
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(readString.Content, 2, readString.Content.Length - 2));
        }
        read = await plc.ReadAsync(address, 1);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        readString = await plc.ReadAsync(address, (ushort)(1 + read.Content[0]));
        if (!readString.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(readString);
        }
        return OperateResult.CreateSuccessResult(encoding.GetString(readString.Content, 1, readString.Content.Length - 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensS7Helper.Write(HslCommunication.Core.IReadWriteNet,HslCommunication.Profinet.Siemens.SiemensPLCS,System.String,System.String,System.Text.Encoding)" />
    public static async Task<OperateResult> WriteAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value, Encoding encoding)
    {
        if (value == null)
        {
            value = string.Empty;
        }
        var buffer = encoding.GetBytes(value);
        if (encoding == Encoding.Unicode)
        {
            buffer = SoftBasic.BytesReverseByWord(buffer);
        }
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            var readLength = await plc.ReadAsync(address, 2);
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
            return await plc.WriteAsync(address, SoftBasic.SpliceArray(new byte[2]
            {
                readLength.Content[0],
                (byte)buffer.Length
            }, buffer));
        }
        return await plc.WriteAsync(address, SoftBasic.SpliceArray(new byte[1] { (byte)buffer.Length }, buffer));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensS7Helper.ReadWString(HslCommunication.Core.IReadWriteNet,HslCommunication.Profinet.Siemens.SiemensPLCS,System.String)" />
    public static async Task<OperateResult<string>> ReadWStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address)
    {
        OperateResult<byte[]> read;
        OperateResult<byte[]> readString;
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            read = await plc.ReadAsync(address, 4);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read);
            }
            readString = await plc.ReadAsync(address, (ushort)(4 + (read.Content[2] * 256 + read.Content[3]) * 2));
            if (!readString.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(readString);
            }
            return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(SoftBasic.BytesReverseByWord(readString.Content.RemoveBegin(4))));
        }
        read = await plc.ReadAsync(address, 1);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        readString = await plc.ReadAsync(address, (ushort)(1 + read.Content[0] * 2));
        if (!readString.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(readString);
        }
        return OperateResult.CreateSuccessResult(Encoding.Unicode.GetString(readString.Content, 1, readString.Content.Length - 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensS7Helper.WriteWString(HslCommunication.Core.IReadWriteNet,HslCommunication.Profinet.Siemens.SiemensPLCS,System.String,System.String)" />
    public static async Task<OperateResult> WriteWStringAsync(IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value)
    {
        if (currentPlc != SiemensPLCS.S200Smart)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            var buffer = Encoding.Unicode.GetBytes(value);
            buffer = SoftBasic.BytesReverseByWord(buffer);
            var readLength = await plc.ReadAsync(address, 4);
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
            return await plc.WriteAsync(address, write);
        }
        return await plc.WriteAsync(address, value, Encoding.Unicode);
    }
}
