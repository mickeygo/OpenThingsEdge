using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// MC协议的辅助类对象，提供了MC协议的读写操作的基本支持
/// </summary>
public class McHelper
{
    /// <summary>
    /// 返回按照字单位读取的最低的长度信息
    /// </summary>
    /// <param name="type">MC协议的类型</param>
    /// <returns>长度信息</returns>
    public static int GetReadWordLength(McType type)
    {
        if (type == McType.McBinary || type == McType.McRBinary)
        {
            return 950;
        }
        return 460;
    }

    /// <summary>
    /// 返回按照位单位读取的最低的长度信息
    /// </summary>
    /// <param name="type">MC协议的类型</param>
    /// <returns>长度信息</returns>
    public static int GetReadBoolLength(McType type)
    {
        if (type == McType.McBinary || type == McType.McRBinary)
        {
            return 7168;
        }
        return 3584;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Read(System.String,System.UInt16)" />
    /// <remarks>
    /// 初步支持普通的数据地址之外，还额外支持高级的地址写法，以下是示例（适用于MC协议为二进制和ASCII格式）：<br />
    /// [商业授权] 扩展的数据地址: 表示为 ext=1;W100  访问扩展区域为1的W100的地址信息<br />
    /// [商业授权] 缓冲存储器地址: 表示为 mem=32  访问地址为32的本站缓冲存储器地址<br />
    /// [商业授权] 智能模块地址：表示为 module=3;4106  访问模块号3，偏移地址是4106的数据，偏移地址需要根据模块的详细信息来确认。<br />
    /// [商业授权] 基于标签的地址: 表示位 s=AAA  假如标签的名称为AAA，但是标签的读取是有条件的，详细参照<see cref="M:HslCommunication.Profinet.Melsec.Helper.McBinaryHelper.ReadTags(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[],System.UInt16[])" /><br />
    /// </remarks>
    public static OperateResult<byte[]> Read(IReadWriteMc mc, string address, ushort length)
    {
        if (mc.McType == McType.McBinary && address.StartsWith("s=") || address.StartsWith("S="))
        {
            return McBinaryHelper.ReadTags(mc, new string[1] { address.Substring(2) }, new ushort[1] { length });
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "ext=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var value = Regex.Match(address, "ext=[0-9]+;").Value;
            var extend = ushort.Parse(Regex.Match(value, "[0-9]+").Value);
            return ReadExtend(mc, extend, address.Substring(value.Length), length);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "mem=", RegexOptions.IgnoreCase))
        {
            return ReadMemory(mc, address.Substring(4), length);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "module=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var value2 = Regex.Match(address, "module=[0-9]+;").Value;
            var module = ushort.Parse(Regex.Match(value2, "[0-9]+").Value);
            return ReadSmartModule(mc, module, address.Substring(value2.Length), (ushort)(length * 2));
        }
        var operateResult = mc.McAnalysisAddress(address, length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var list = new List<byte>();
        ushort num = 0;
        while (num < length)
        {
            var num2 = (ushort)Math.Min(length - num, GetReadWordLength(mc.McType));
            operateResult.Content.Length = num2;
            var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreCommand(operateResult.Content, isBit: false) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: false) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildReadMcCoreCommand(operateResult.Content, isBit: false) : null;
            OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
            if (!operateResult2.IsSuccess)
            {
                return operateResult2;
            }
            list.AddRange(mc.ExtractActualData(operateResult2.Content, isBit: false));
            num += num2;
            if (operateResult.Content.McDataType.DataType == 0)
            {
                operateResult.Content.AddressStart += num2;
            }
            else
            {
                operateResult.Content.AddressStart += num2 * 16;
            }
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Byte[])" />
    public static OperateResult Write(IReadWriteMc mc, string address, byte[] value)
    {
        var operateResult = mc.McAnalysisAddress(address, 0, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildWriteWordCoreCommand(operateResult.Content, value) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiWriteWordCoreCommand(operateResult.Content, value) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildWriteWordCoreCommand(operateResult.Content, value) : null;
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBool(System.String)" />
    /// <remarks>
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010
    /// </remarks>
    public static OperateResult<bool> ReadBool(IReadWriteMc mc, string address)
    {
        return ByteTransformHelper.GetResultFromArray(ReadBool(mc, address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBool(System.String,System.UInt16)" />
    /// <remarks>
    /// 当读取的长度过大时，会自动进行切割，对于二进制格式，切割长度为7168，对于ASCII格式协议来说，切割长度则是3584<br />
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010
    /// </remarks>
    public static OperateResult<bool[]> ReadBool(IReadWriteMc mc, string address, ushort length, bool supportWordAdd = true)
    {
        if (supportWordAdd && address.IndexOf('.') > 0)
        {
            return CommunicationHelper.ReadBool(mc, address, length);
        }
        var operateResult = mc.McAnalysisAddress(address, length, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var list = new List<bool>();
        ushort num = 0;
        while (num < length)
        {
            var num2 = (ushort)Math.Min(length - num, GetReadBoolLength(mc.McType));
            operateResult.Content.Length = num2;
            var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreCommand(operateResult.Content, isBit: true) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: true) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildReadMcCoreCommand(operateResult.Content, isBit: true) : null;
            OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult2);
            }
            list.AddRange((from m in mc.ExtractActualData(operateResult2.Content, isBit: true)
                           select m == 1).Take(num2).ToArray());
            num += num2;
            operateResult.Content.AddressStart += num2;
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean[])" />
    /// <remarks>
    /// 当读取的长度过大时，会自动进行切割，对于二进制格式，切割长度为7168，对于ASCII格式协议来说，切割长度则是3584<br />
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010
    /// </remarks>
    public static OperateResult Write(IReadWriteMc mc, string address, bool[] values, bool supportWordAdd = true)
    {
        if (supportWordAdd && mc.EnableWriteBitToWordRegister && address.Contains("."))
        {
            return ReadWriteNetHelper.WriteBoolWithWord(mc, address, values);
        }
        var operateResult = mc.McAnalysisAddress(address, 0, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildWriteBitCoreCommand(operateResult.Content, values) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiWriteBitCoreCommand(operateResult.Content, values) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildWriteBitCoreCommand(operateResult.Content, values) : null;
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.Read(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteMc mc, string address, ushort length)
    {
        if (mc.McType == McType.McBinary && address.StartsWith("s=") || address.StartsWith("S="))
        {
            return await McBinaryHelper.ReadTagsAsync(mc, new string[1] { address.Substring(2) }, new ushort[1] { length });
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "ext=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var extStr = Regex.Match(address, "ext=[0-9]+;").Value;
            var ext = ushort.Parse(Regex.Match(extStr, "[0-9]+").Value);
            return await ReadExtendAsync(mc, ext, address.Substring(extStr.Length), length);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "mem=", RegexOptions.IgnoreCase))
        {
            return await ReadMemoryAsync(mc, address.Substring(4), length);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "module=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var moduleStr = Regex.Match(address, "module=[0-9]+;").Value;
            var module = ushort.Parse(Regex.Match(moduleStr, "[0-9]+").Value);
            return await ReadSmartModuleAsync(mc, module, address.Substring(moduleStr.Length), (ushort)(length * 2));
        }
        var addressResult = mc.McAnalysisAddress(address, length, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        var bytesContent = new List<byte>();
        ushort alreadyFinished = 0;
        while (alreadyFinished < length)
        {
            var readLength = (ushort)Math.Min(length - alreadyFinished, GetReadWordLength(mc.McType));
            addressResult.Content.Length = readLength;
            var command = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreCommand(addressResult.Content, isBit: false) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: false) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildReadMcCoreCommand(addressResult.Content, isBit: false) : null;
            var read = await mc.ReadFromCoreServerAsync(command);
            if (!read.IsSuccess)
            {
                return read;
            }
            bytesContent.AddRange(mc.ExtractActualData(read.Content, isBit: false));
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

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Byte[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteMc mc, string address, byte[] value)
    {
        var addressResult = mc.McAnalysisAddress(address, 0, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildWriteWordCoreCommand(addressResult.Content, value) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildWriteWordCoreCommand(addressResult.Content, value) : null;
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadBool(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16,System.Boolean)" />
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteMc mc, string address, ushort length, bool supportWordAdd = true)
    {
        if (supportWordAdd && address.IndexOf('.') > 0)
        {
            return await CommunicationHelper.ReadBoolAsync(mc, address, length);
        }
        var addressResult = mc.McAnalysisAddress(address, length, isBit: true);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(addressResult);
        }
        var boolContent = new List<bool>();
        ushort alreadyFinished = 0;
        while (alreadyFinished < length)
        {
            var readLength = (ushort)Math.Min(length - alreadyFinished, GetReadBoolLength(mc.McType));
            addressResult.Content.Length = readLength;
            var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreCommand(addressResult.Content, isBit: true) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: true) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildReadMcCoreCommand(addressResult.Content, isBit: true) : null;
            var read = await mc.ReadFromCoreServerAsync(coreResult);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            boolContent.AddRange((from m in mc.ExtractActualData(read.Content, isBit: true)
                                  select m == 1).Take(readLength).ToArray());
            alreadyFinished += readLength;
            addressResult.Content.AddressStart += readLength;
        }
        return OperateResult.CreateSuccessResult(boolContent.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteMc mc, string address, bool[] values, bool supportWordAdd = true)
    {
        if (supportWordAdd && mc.EnableWriteBitToWordRegister && address.Contains("."))
        {
            return await ReadWriteNetHelper.WriteBoolWithWordAsync(mc, address, values).ConfigureAwait(continueOnCapturedContext: false);
        }
        var addressResult = mc.McAnalysisAddress(address, 0, isBit: true);
        if (!addressResult.IsSuccess)
        {
            return addressResult;
        }
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildWriteBitCoreCommand(addressResult.Content, values) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, values) : mc.McType == McType.McRBinary ? MelsecMcRNet.BuildWriteBitCoreCommand(addressResult.Content, values) : null;
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，需要自行解析数据<br />
    /// Randomly read PLC data information, which can be combined across addresses and types, but each address can only read one word, 
    /// which is the content of 2 bytes. After receiving the results, you need to parse the data yourself
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
    /// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
    /// <br />
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
    /// <br />
    /// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
    /// </remarks>
    /// <example>
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" /></example>
    /// <returns>结果</returns>
    public static OperateResult<byte[]> ReadRandom(IReadWriteMc mc, string[] address)
    {
        var array = new McAddressData[address.Length];
        for (var i = 0; i < address.Length; i++)
        {
            var operateResult = McAddressData.ParseMelsecFrom(address[i], 1, isBit: false);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }
            array[i] = operateResult.Content;
        }
        var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadRandomWordCommand(array) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomWordCommand(array) : null;
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(operateResult2.Content, isBit: false));
    }

    /// <summary>
    /// 使用块读取PLC的数据信息，可以跨地址，跨类型组合，每个地址是任意的长度。收到结果后，需要自行解析数据，目前只支持字地址，比如D区，W区，R区，不支持X，Y，M，B，L等等<br />
    /// Read the data information of the PLC randomly. It can be combined across addresses and types. Each address is of any length. After receiving the results, 
    /// you need to parse the data yourself. Currently, only word addresses are supported, such as D area, W area, R area. X, Y, M, B, L, etc
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <param name="length">每个地址的长度信息</param>
    /// <remarks>
    /// 实际测试不一定所有的plc都可以读取成功，具体情况需要具体分析
    /// <br />
    /// 1 块数按照下列要求指定 120 ≧ 字软元件块数 + 位软元件块数
    /// <br />
    /// 2 各软元件点数按照下列要求指定 960 ≧ 字软元件各块的合计点数 + 位软元件各块的合计点数
    /// </remarks>
    /// <example>
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
    /// </example>
    /// <returns>结果</returns>
    public static OperateResult<byte[]> ReadRandom(IReadWriteMc mc, string[] address, ushort[] length)
    {
        if (length.Length != address.Length)
        {
            return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var array = new McAddressData[address.Length];
        for (var i = 0; i < address.Length; i++)
        {
            var operateResult = McAddressData.ParseMelsecFrom(address[i], length[i], isBit: false);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }
            array[i] = operateResult.Content;
        }
        var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadRandomCommand(array) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomCommand(array) : null;
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(operateResult2.Content, isBit: false));
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了short类型的数组<br />
    /// Randomly read PLC data information, which can be combined across addresses and types, but each address can only read one word, 
    /// which is the content of 2 bytes. After receiving the result, it is automatically converted to an array of type short.
    /// </summary>
    /// <param name="mc">MC协议的通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
    /// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
    ///
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
    ///
    /// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
    /// </remarks>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<short[]> ReadRandomInt16(IReadWriteMc mc, string[] address)
    {
        var operateResult = ReadRandom(mc, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<short[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransInt16(operateResult.Content, 0, address.Length));
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了ushort类型的数组<br />
    /// Randomly read PLC data information, which can be combined across addresses and types, but each address can only read one word, 
    /// which is the content of 2 bytes. After receiving the result, it is automatically converted to an array of type ushort.
    /// </summary>
    /// <param name="mc">MC协议的通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
    /// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
    ///
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
    ///
    /// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
    /// </remarks>
    /// <returns>包含是否成功的结果对象</returns>
    public static OperateResult<ushort[]> ReadRandomUInt16(IReadWriteMc mc, string[] address)
    {
        var operateResult = ReadRandom(mc, address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransUInt16(operateResult.Content, 0, address.Length));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandom(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    public static async Task<OperateResult<byte[]>> ReadRandomAsync(IReadWriteMc mc, string[] address)
    {
        var mcAddressDatas = new McAddressData[address.Length];
        for (var i = 0; i < address.Length; i++)
        {
            var addressResult = McAddressData.ParseMelsecFrom(address[i], 1, isBit: false);
            if (!addressResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(addressResult);
            }
            mcAddressDatas[i] = addressResult.Content;
        }
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadRandomWordCommand(mcAddressDatas) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas) : null;
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandom(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[],System.UInt16[])" />
    public static async Task<OperateResult<byte[]>> ReadRandomAsync(IReadWriteMc mc, string[] address, ushort[] length)
    {
        if (length.Length != address.Length)
        {
            return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var mcAddressDatas = new McAddressData[address.Length];
        for (var i = 0; i < address.Length; i++)
        {
            var addressResult = McAddressData.ParseMelsecFrom(address[i], length[i], isBit: false);
            if (!addressResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(addressResult);
            }
            mcAddressDatas[i] = addressResult.Content;
        }
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadRandomCommand(mcAddressDatas) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomCommand(mcAddressDatas) : null;
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandomInt16(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    public static async Task<OperateResult<short[]>> ReadRandomInt16Async(IReadWriteMc mc, string[] address)
    {
        var read = await ReadRandomAsync(mc, address);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<short[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransInt16(read.Content, 0, address.Length));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadRandomUInt16(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[])" />
    public static async Task<OperateResult<ushort[]>> ReadRandomUInt16Async(IReadWriteMc mc, string[] address)
    {
        var read = await ReadRandomAsync(mc, address);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransUInt16(read.Content, 0, address.Length));
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取缓冲寄存器的数据信息，地址直接为偏移地址<br />
    /// <b>[Authorization]</b> Read the data information of the buffer register, the address is directly the offset address
    /// </summary>
    /// <remarks>
    /// 本指令不可以访问下述缓冲存储器:<br />
    /// 1. 本站(SLMP对应设备)上安装的智能功能模块<br />
    /// 2. 其它站缓冲存储器<br />
    /// </remarks>
    /// <param name="mc">MC通信对象</param>
    /// <param name="address">偏移地址</param>
    /// <param name="length">读取长度</param>
    /// <returns>读取的内容</returns>
    public static OperateResult<byte[]> ReadMemory(IReadWriteMc mc, string address, ushort length)
    {
        var operateResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMemoryCommand(address, length) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMemoryCommand(address, length) : null;
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(operateResult2.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadMemory(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadMemoryAsync(IReadWriteMc mc, string address, ushort length)
    {
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMemoryCommand(address, length) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMemoryCommand(address, length) : null;
        if (!coreResult.IsSuccess)
        {
            return coreResult;
        }
        var read = await mc.ReadFromCoreServerAsync(coreResult.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取智能模块的数据信息，需要指定模块地址，偏移地址，读取的字节长度<br />
    /// <b>[Authorization]</b> To read the extended data information, you need to enter the extended value information in addition to the original address and length information
    /// </summary>
    /// <param name="mc">MC通信对象</param>
    /// <param name="module">模块地址</param>
    /// <param name="address">地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>返回结果</returns>
    public static OperateResult<byte[]> ReadSmartModule(IReadWriteMc mc, ushort module, string address, ushort length)
    {
        var operateResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadSmartModule(module, address, length) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadSmartModule(module, address, length) : null;
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(operateResult2.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadSmartModule(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.UInt16,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadSmartModuleAsync(IReadWriteMc mc, ushort module, string address, ushort length)
    {
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadSmartModule(module, address, length) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadSmartModule(module, address, length) : null;
        if (!coreResult.IsSuccess)
        {
            return coreResult;
        }
        var read = await mc.ReadFromCoreServerAsync(coreResult.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取扩展的数据信息，需要在原有的地址，长度信息之外，输入扩展值信息<br />
    /// <b>[Authorization]</b> To read the extended data information, you need to enter the extended value information in addition to the original address and length information
    /// </summary>
    /// <param name="mc">MC通信对象</param>
    /// <param name="extend">扩展信息</param>
    /// <param name="address">地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>返回结果</returns>
    public static OperateResult<byte[]> ReadExtend(IReadWriteMc mc, ushort extend, string address, ushort length)
    {
        var operateResult = mc.McAnalysisAddress(address, length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var send = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreExtendCommand(operateResult.Content, extend, isBit: false) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreExtendCommand(operateResult.Content, extend, isBit: false) : null;
        OperateResult<byte[]> operateResult2 = mc.ReadFromCoreServer(send);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(operateResult2.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadExtend(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.UInt16,System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadExtendAsync(IReadWriteMc mc, ushort extend, string address, ushort length)
    {
        var addressResult = mc.McAnalysisAddress(address, length, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        var coreResult = mc.McType == McType.McBinary ? McBinaryHelper.BuildReadMcCoreExtendCommand(addressResult.Content, extend, isBit: false) : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreExtendCommand(addressResult.Content, extend, isBit: false) : null;
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 远程Run操作<br />
    /// Remote Run Operation
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static OperateResult RemoteRun(IReadWriteMc mc)
    {
        return mc.McType == McType.McBinary ? mc.ReadFromCoreServer(new byte[8] { 1, 16, 0, 0, 1, 0, 0, 0 }) : mc.McType == McType.MCAscii ? mc.ReadFromCoreServer(Encoding.ASCII.GetBytes("1001000000010000")) : new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
    }

    /// <summary>
    /// 远程Stop操作<br />
    /// Remote Stop operation
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static OperateResult RemoteStop(IReadWriteMc mc)
    {
        return mc.McType == McType.McBinary ? mc.ReadFromCoreServer(new byte[6] { 2, 16, 0, 0, 1, 0 }) : mc.McType == McType.MCAscii ? mc.ReadFromCoreServer(Encoding.ASCII.GetBytes("100200000001")) : new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
    }

    /// <summary>
    /// 远程Reset操作<br />
    /// Remote Reset Operation
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static OperateResult RemoteReset(IReadWriteMc mc)
    {
        return mc.McType == McType.McBinary ? mc.ReadFromCoreServer(new byte[6] { 6, 16, 0, 0, 1, 0 }) : mc.McType == McType.MCAscii ? mc.ReadFromCoreServer(Encoding.ASCII.GetBytes("100600000001")) : new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
    }

    /// <summary>
    /// 读取PLC的型号信息，例如 Q02HCPU<br />
    /// Read PLC model information, such as Q02HCPU
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>返回型号的结果对象</returns>
    public static OperateResult<string> ReadPlcType(IReadWriteMc mc)
    {
        OperateResult<byte[]> operateResult = mc.McType == McType.McBinary ? mc.ReadFromCoreServer(new byte[4] { 1, 1, 0, 0 }) : mc.McType == McType.MCAscii ? mc.ReadFromCoreServer(Encoding.ASCII.GetBytes("01010000")) : new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult.Content, 0, 16).TrimEnd());
    }

    /// <summary>
    /// LED 熄灭 出错代码初始化<br />
    /// LED off Error code initialization
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static OperateResult ErrorStateReset(IReadWriteMc mc)
    {
        return mc.McType == McType.McBinary ? mc.ReadFromCoreServer(new byte[4] { 23, 22, 0, 0 }) : mc.McType == McType.MCAscii ? mc.ReadFromCoreServer(Encoding.ASCII.GetBytes("16170000")) : new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteRun(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    public static async Task<OperateResult> RemoteRunAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync(new byte[8] { 1, 16, 0, 0, 1, 0, 0, 0 });
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction) : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("1001000000010000"));
            result = operateResult;
        }
        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteStop(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    public static async Task<OperateResult> RemoteStopAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync(new byte[6] { 2, 16, 0, 0, 1, 0 });
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction) : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("100200000001"));
            result = operateResult;
        }
        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.RemoteReset(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    public static async Task<OperateResult> RemoteResetAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync(new byte[6] { 6, 16, 0, 0, 1, 0 });
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction) : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("100600000001"));
            result = operateResult;
        }
        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ReadPlcType(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> operateResult;
        if (mc.McType == McType.McBinary)
        {
            operateResult = await mc.ReadFromCoreServerAsync(new byte[4] { 1, 1, 0, 0 });
        }
        else
        {
            var operateResult2 = mc.McType != McType.MCAscii ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction) : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("01010000"));
            operateResult = operateResult2;
        }
        var read = operateResult;
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 0, 16).TrimEnd());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McHelper.ErrorStateReset(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc)" />
    public static async Task<OperateResult> ErrorStateResetAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync(new byte[4] { 23, 22, 0, 0 });
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction) : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("16170000"));
            result = operateResult;
        }
        return result;
    }
}
