using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// MC协议的辅助类对象，提供了MC协议的读写操作的基本支持。
/// </summary>
public static class McHelper
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

    /// <inheritdoc cref="IReadWriteNet.ReadAsync" />
    /// <remarks>
    /// 初步支持普通的数据地址之外，还额外支持高级的地址写法，以下是示例（适用于MC协议为二进制和ASCII格式）：
    /// 扩展的数据地址: 表示为 ext=1;W100  访问扩展区域为1的W100的地址信息；
    /// 缓冲存储器地址: 表示为 mem=32  访问地址为32的本站缓冲存储器地址；
    /// 智能模块地址：表示为 module=3;4106  访问模块号3，偏移地址是4106的数据，偏移地址需要根据模块的详细信息来确认；
    /// 基于标签的地址: 表示位 s=AAA  假如标签的名称为AAA，但是标签的读取是有条件的。
    /// </remarks>
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteMc mc, string address, ushort length)
    {
        if (mc.McType == McType.McBinary && address.StartsWith("s=") || address.StartsWith("S="))
        {
            return await McBinaryHelper.ReadTagsAsync(mc, [address.Substring(2)], [length]).ConfigureAwait(false);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "ext=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var extStr = Regex.Match(address, "ext=[0-9]+;").Value;
            var ext = ushort.Parse(Regex.Match(extStr, "[0-9]+").Value);
            return await ReadExtendAsync(mc, ext, address[extStr.Length..], length).ConfigureAwait(false);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "mem=", RegexOptions.IgnoreCase))
        {
            return await ReadMemoryAsync(mc, address[4..], length).ConfigureAwait(false);
        }
        if ((mc.McType == McType.McBinary || mc.McType == McType.MCAscii) && Regex.IsMatch(address, "module=[0-9]+;", RegexOptions.IgnoreCase))
        {
            var moduleStr = Regex.Match(address, "module=[0-9]+;").Value;
            var module = ushort.Parse(Regex.Match(moduleStr, "[0-9]+").Value);
            return await ReadSmartModuleAsync(mc, module, address.Substring(moduleStr.Length), (ushort)(length * 2)).ConfigureAwait(false);
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
            var command = mc.McType == McType.McBinary
                ? McBinaryHelper.BuildReadMcCoreCommand(addressResult.Content, isBit: false)
                : mc.McType == McType.MCAscii
                    ? McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: false)
                    : mc.McType == McType.McRBinary ? BuildReadMcCoreCommand(addressResult.Content, isBit: false) : [];
            var read = await mc.ReadFromCoreServerAsync(command).ConfigureAwait(false);
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

    /// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string)" />
    /// <remarks>
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010。
    /// </remarks>
    public static async Task<OperateResult<bool>> ReadBoolAsync(IReadWriteMc mc, string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadBoolAsync(mc, address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string,ushort)" />
    /// <remarks>
    /// 当读取的长度过大时，会自动进行切割，对于二进制格式，切割长度为7168，对于ASCII格式协议来说，切割长度则是3584；
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010。
    /// </remarks>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IReadWriteMc mc, string address, ushort length, bool supportWordAdd = true)
    {
        if (supportWordAdd && address.IndexOf('.') > 0)
        {
            return await CommHelper.ReadBoolAsync(mc, address, length).ConfigureAwait(false);
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
            var coreResult = mc.McType == McType.McBinary
                ? McBinaryHelper.BuildReadMcCoreCommand(addressResult.Content, isBit: true)
                : mc.McType == McType.MCAscii
                    ? McAsciiHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, isBit: true)
                    : mc.McType == McType.McRBinary ? BuildReadMcCoreCommand(addressResult.Content, isBit: true) : [];
            var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
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

    /// <inheritdoc cref="IReadWriteNet.WriteAsync(string,bool[])" />
    public static async Task<OperateResult> WriteAsync(IReadWriteMc mc, string address, byte[] values)
    {
        var addressResult = mc.McAnalysisAddress(address, 0, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }

        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildWriteWordCoreCommand(addressResult.Content, values)
            : mc.McType == McType.MCAscii
                ? McAsciiHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, values)
                : mc.McType == McType.McRBinary ? BuildWriteWordCoreCommand(addressResult.Content, values) : [];
        var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="IReadWriteNet.WriteAsync(string,bool[])" />
    /// <remarks>
    /// 当读取的长度过大时，会自动进行切割，对于二进制格式，切割长度为7168，对于ASCII格式协议来说，切割长度则是3584；
    /// 对于X,Y类型的地址，有两种表示方式，十六进制和八进制，默认16进制，比如输入 X10 是16进制的，如果想要输入8进制的地址，地址补0操作，例如 X010。
    /// </remarks>
    public static async Task<OperateResult> WriteAsync(IReadWriteMc mc, string address, bool[] values, bool supportWordAdd = true)
    {
        if (supportWordAdd && mc.EnableWriteBitToWordRegister && address.Contains('.'))
        {
            return await ReadWriteNetHelper.WriteBoolWithWordAsync(mc, address, values).ConfigureAwait(continueOnCapturedContext: false);
        }

        var addressResult = mc.McAnalysisAddress(address, 0, isBit: true);
        if (!addressResult.IsSuccess)
        {
            return addressResult;
        }
        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildWriteBitCoreCommand(addressResult.Content, values)
            : mc.McType == McType.MCAscii
                ? McAsciiHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, values)
                : mc.McType == McType.McRBinary ? BuildWriteBitCoreCommand(addressResult.Content, values) : [];
        var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，需要自行解析数据。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站时访问点数，字访问点数 双字访问点数不大于 192；
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数 -> 字访问点数 双字访问点数不大于 96；
    /// 访问上述以外的 PLC CPU 其他站 时访问点数，字访问点数不大于 10。
    /// </remarks>
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
        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildReadRandomWordCommand(mcAddressDatas)
            : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas) : [];
        var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 使用块读取PLC的数据信息，可以跨地址，跨类型组合，每个地址是任意的长度。收到结果后，需要自行解析数据，目前只支持字地址，比如D区，W区，R区，不支持X，Y，M，B，L等等。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <param name="length">每个地址的长度信息</param>
    /// <remarks>
    /// 实际测试不一定所有的plc都可以读取成功，具体情况需要具体分析；
    /// 1、块数按照下列要求指定 120 字软元件块数 + 位软元件块数；
    /// 2、各软元件点数按照下列要求指定 960 字软元件各块的合计点数 + 位软元件各块的合计点数。
    /// </remarks>
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

        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildReadRandomCommand(mcAddressDatas)
            : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadRandomCommand(mcAddressDatas) : [];
        var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了short类型的数组。
    /// </summary>
    /// <param name="mc">MC协议的通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站时访问点数，字访问点数 双字访问点数不大于 192；
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数 -> 字访问点数 双字访问点数不大于 96；
    /// 访问上述以外的 PLC CPU 其他站 时访问点数，字访问点数不大于 10。
    /// </remarks>
    /// <returns>包含是否成功的结果对象</returns>
    public static async Task<OperateResult<short[]>> ReadRandomInt16Async(IReadWriteMc mc, string[] address)
    {
        var read = await ReadRandomAsync(mc, address).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<short[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransInt16(read.Content, 0, address.Length));
    }

    /// <summary>
    /// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了ushort类型的数组。
    /// </summary>
    /// <param name="mc">MC协议的通信对象</param>
    /// <param name="address">所有的地址的集合</param>
    /// <remarks>
    /// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站时访问点数，字访问点数 双字访问点数不大于 192；
    /// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数 -> 字访问点数 双字访问点数不大于 96；
    /// 访问上述以外的 PLC CPU 其他站 时访问点数，字访问点数不大于 10。
    /// </remarks>
    /// <returns>包含是否成功的结果对象</returns>
    public static async Task<OperateResult<ushort[]>> ReadRandomUInt16Async(IReadWriteMc mc, string[] address)
    {
        var read = await ReadRandomAsync(mc, address).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ByteTransform.TransUInt16(read.Content, 0, address.Length));
    }

    /// <summary>
    /// 读取缓冲寄存器的数据信息，地址直接为偏移地址。
    /// </summary>
    /// <remarks>
    /// 本指令不可以访问下述缓冲存储器:
    /// 1. 本站(SLMP对应设备)上安装的智能功能模块；
    /// 2. 其它站缓冲存储器。
    /// </remarks>
    /// <param name="mc">MC通信对象</param>
    /// <param name="address">偏移地址</param>
    /// <param name="length">读取长度</param>
    /// <returns>读取的内容</returns>
    public static async Task<OperateResult<byte[]>> ReadMemoryAsync(IReadWriteMc mc, string address, ushort length)
    {
        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildReadMemoryCommand(address, length)
            : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMemoryCommand(address, length) : null;
        if (!coreResult.IsSuccess)
        {
            return coreResult;
        }

        var read = await mc.ReadFromCoreServerAsync(coreResult.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 读取智能模块的数据信息，需要指定模块地址，偏移地址，读取的字节长度。
    /// </summary>
    /// <param name="mc">MC通信对象</param>
    /// <param name="module">模块地址</param>
    /// <param name="address">地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>返回结果</returns>
    public static async Task<OperateResult<byte[]>> ReadSmartModuleAsync(IReadWriteMc mc, ushort module, string address, ushort length)
    {
        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildReadSmartModule(module, address, length)
            : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadSmartModule(module, address, length) : null;
        if (!coreResult.IsSuccess)
        {
            return coreResult;
        }

        var read = await mc.ReadFromCoreServerAsync(coreResult.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 读取扩展的数据信息，需要在原有的地址，长度信息之外，输入扩展值信息。
    /// </summary>
    /// <param name="mc">MC通信对象</param>
    /// <param name="extend">扩展信息</param>
    /// <param name="address">地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>返回结果</returns>
    public static async Task<OperateResult<byte[]>> ReadExtendAsync(IReadWriteMc mc, ushort extend, string address, ushort length)
    {
        var addressResult = mc.McAnalysisAddress(address, length, isBit: false);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        var coreResult = mc.McType == McType.McBinary
            ? McBinaryHelper.BuildReadMcCoreExtendCommand(addressResult.Content, extend, isBit: false)
            : mc.McType == McType.MCAscii ? McAsciiHelper.BuildAsciiReadMcCoreExtendCommand(addressResult.Content, extend, isBit: false) : [];
        var read = await mc.ReadFromCoreServerAsync(coreResult).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(mc.ExtractActualData(read.Content, isBit: false));
    }

    /// <summary>
    /// 远程Run操作。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> RemoteRunAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync([1, 16, 0, 0, 1, 0, 0, 0]).ConfigureAwait(false);
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii
                ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction)
                : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("1001000000010000")).ConfigureAwait(false);
            result = operateResult;
        }
        return result;
    }

    /// <summary>
    /// 远程Stop操作。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> RemoteStopAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync([2, 16, 0, 0, 1, 0]).ConfigureAwait(false);
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii
                ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction)
                : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("100200000001")).ConfigureAwait(false);
            result = operateResult;
        }
        return result;
    }

    /// <summary>
    /// 远程Reset操作。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> RemoteResetAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync([6, 16, 0, 0, 1, 0]).ConfigureAwait(false);
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii
                ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction)
                : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("100600000001")).ConfigureAwait(false);
            result = operateResult;
        }
        return result;
    }

    /// <summary>
    /// 读取PLC的型号信息，例如 Q02HCPU。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>返回型号的结果对象</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> operateResult;
        if (mc.McType == McType.McBinary)
        {
            operateResult = await mc.ReadFromCoreServerAsync([1, 1, 0, 0]).ConfigureAwait(false);
        }
        else
        {
            var operateResult2 = mc.McType != McType.MCAscii
                ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction)
                : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("01010000")).ConfigureAwait(false);
            operateResult = operateResult2;
        }
        var read = operateResult;
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 0, 16).TrimEnd());
    }

    /// <summary>
    /// LED 熄灭 出错代码初始化。
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <returns>是否成功</returns>
    public static async Task<OperateResult> ErrorStateResetAsync(IReadWriteMc mc)
    {
        OperateResult<byte[]> result;
        if (mc.McType == McType.McBinary)
        {
            result = await mc.ReadFromCoreServerAsync([23, 22, 0, 0]).ConfigureAwait(false);
        }
        else
        {
            var operateResult = mc.McType != McType.MCAscii
                ? new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction)
                : await mc.ReadFromCoreServerAsync(Encoding.ASCII.GetBytes("16170000")).ConfigureAwait(false);
            result = operateResult;
        }
        return result;
    }

    /// <summary>
    /// 从三菱地址，是否位读取进行创建读取的MC的核心报文
    /// </summary>
    /// <param name="address">地址数据</param>
    /// <param name="isBit">是否进行了位读取操作</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildReadMcCoreCommand(McAddressData address, bool isBit)
    {
        return
        [
            1,
            4,
            (byte)(isBit ? 1 : 0),
            0,
            BitConverter.GetBytes(address.AddressStart)[0],
            BitConverter.GetBytes(address.AddressStart)[1],
            BitConverter.GetBytes(address.AddressStart)[2],
            BitConverter.GetBytes(address.AddressStart)[3],
            BitConverter.GetBytes(address.McDataType.DataCode)[0],
            BitConverter.GetBytes(address.McDataType.DataCode)[1],
            (byte)(address.Length % 256),
            (byte)(address.Length / 256)
        ];
    }

    /// <summary>
    /// 以字为单位，创建数据写入的核心报文
    /// </summary>
    /// <param name="address">三菱的数据地址</param>
    /// <param name="value">实际的原始数据信息</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildWriteWordCoreCommand(McAddressData address, byte[] value)
    {
        value ??= [];
        var array = new byte[12 + value.Length];
        array[0] = 1;
        array[1] = 20;
        array[2] = 0;
        array[3] = 0;
        array[4] = BitConverter.GetBytes(address.AddressStart)[0];
        array[5] = BitConverter.GetBytes(address.AddressStart)[1];
        array[6] = BitConverter.GetBytes(address.AddressStart)[2];
        array[7] = BitConverter.GetBytes(address.AddressStart)[3];
        array[8] = BitConverter.GetBytes(address.McDataType.DataCode)[0];
        array[9] = BitConverter.GetBytes(address.McDataType.DataCode)[1];
        array[10] = (byte)(value.Length / 2 % 256);
        array[11] = (byte)(value.Length / 2 / 256);
        value.CopyTo(array, 12);
        return array;
    }

    /// <summary>
    /// 以位为单位，创建数据写入的核心报文
    /// </summary>
    /// <param name="address">三菱的地址信息</param>
    /// <param name="value">原始的bool数组数据</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildWriteBitCoreCommand(McAddressData address, bool[] value)
    {
        value ??= [];
        var array = MelsecHelper.TransBoolArrayToByteData(value);
        var array2 = new byte[12 + array.Length];
        array2[0] = 1;
        array2[1] = 20;
        array2[2] = 1;
        array2[3] = 0;
        array2[4] = BitConverter.GetBytes(address.AddressStart)[0];
        array2[5] = BitConverter.GetBytes(address.AddressStart)[1];
        array2[6] = BitConverter.GetBytes(address.AddressStart)[2];
        array2[7] = BitConverter.GetBytes(address.AddressStart)[3];
        array2[8] = BitConverter.GetBytes(address.McDataType.DataCode)[0];
        array2[9] = BitConverter.GetBytes(address.McDataType.DataCode)[1];
        array2[10] = (byte)(value.Length % 256);
        array2[11] = (byte)(value.Length / 256);
        array.CopyTo(array2, 12);
        return array2;
    }
}
