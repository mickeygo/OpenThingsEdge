using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 基于连接的对象访问的CIP协议的实现，用于对Omron PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。
/// </summary>
/// <remarks>
/// 支持普通标签的读写，类型要和标签对应上。如果标签是数组，例如 A 是 INT[0...9] 那么Read("A", 1)，返回的是10个short所有字节数组。
/// 如果需要返回10个长度的short数组，请调用 ReadInt16("A[0], 10"); 地址必须写 "A[0]"，不能写 "A"。
/// </remarks>
public class OmronConnectedCipNet : NetworkConnectedCip, IReadWriteCip, IReadWriteNet
{
    /// <summary>
    /// 当前产品的型号信息。
    /// </summary>
    public string? ProductName { get; private set; }

    /// <summary>
    /// 获取或设置不通信时超时的时间，默认02，为 32 秒，设置06 时为8分多，计算方法为 (2的x次方乘以8) 的秒数。
    /// </summary>
    public byte ConnectionTimeoutMultiplier { get; set; } = 2;

    /// <summary>
    /// 根据指定的IP及端口来实例化这个连接对象
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口号信息</param>
    public OmronConnectedCipNet(string ipAddress, int port = 44818) : base(ipAddress, port)
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
    }

    /// <inheritdoc />
    protected override byte[] GetLargeForwardOpen(ushort connectionID)
    {
        var value = TOConnectionId = (uint)(-2130837503 + connectionID);
        var array = "\r\n00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00\r\n5b 02 20 06 24 01 0e 9c 02 00 00 80 01 00 fe 80\r\n02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00\r\ncc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02\r\n24 01 2c 01".ToHexBytes();
        BitConverter.GetBytes((uint)(-2147483646 + connectionID)).CopyTo(array, 24);
        BitConverter.GetBytes(value).CopyTo(array, 28);
        BitConverter.GetBytes((ushort)(2 + connectionID)).CopyTo(array, 32);
        BitConverter.GetBytes((ushort)4105).CopyTo(array, 34);
        CommunicationHelper.HslRandom.GetBytes(4).CopyTo(array, 36);
        array[40] = ConnectionTimeoutMultiplier;
        return array;
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var ini = await base.InitializationOnConnectAsync().ConfigureAwait(false);
        if (!ini.IsSuccess)
        {
            return ini;
        }

        var read = await ReadFromCoreServerAsync(Pipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetAttributeAll()), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (read.Content.Length > 59 && read.Content.Length >= 59 + read.Content[58])
        {
            ProductName = Encoding.UTF8.GetString(read.Content, 59, read.Content[58]);
        }
        return OperateResult.CreateSuccessResult();
    }

    private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string[] address, ushort[] length)
    {
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(read);
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(check);
        }
        return ExtractActualData(read.Content, isRead: true);
    }

    public async Task<OperateResult<byte[]>> ReadCipFromServerAsync(params byte[][] cips)
    {
        var command = PackCommandService([.. cips]);
        var read = await ReadFromCoreServerAsync(command).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(read.Content);
    }

    /// <summary>
    /// 读取一个结构体的对象，需要事先根据实际的数据点位定义好结构体，然后使用本方法进行读取，当结构体定义不对时，本方法将会读取失败。
    /// </summary>
    /// <typeparam name="T">结构体的类型</typeparam>
    /// <param name="address">结构体对象的地址</param>
    /// <returns>是否读取成功的对象</returns>
    public async Task<OperateResult<T>> ReadStructAsync<T>(string address) where T : struct
    {
        var read = await ReadAsync(address, 1).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<T>(read);
        }
        return CommunicationHelper.ByteArrayToStruct<T>(read.Content.RemoveBegin(2));
    }

    /// <summary>
    /// 获取传递的最大长度的字节信息
    /// </summary>
    /// <returns>字节长度</returns>
    protected virtual int GetMaxTransferBytes()
    {
        return 1988;
    }

    private int GetLengthFromRemain(ushort dataType, int length)
    {
        if (dataType == 193 || dataType == 194 || dataType == 198 || dataType == 211)
        {
            return Math.Min(length, GetMaxTransferBytes());
        }
        if (dataType == 199 || dataType == 195)
        {
            return Math.Min(length, GetMaxTransferBytes() / 2);
        }
        if (dataType == 196 || dataType == 200 || dataType == 202)
        {
            return Math.Min(length, GetMaxTransferBytes() / 4);
        }
        return Math.Min(length, GetMaxTransferBytes() / 8);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return Task.FromResult(OperateResult.CreateSuccessResult(ProductName ?? ""));
    }

    /// <summary>
    /// 读取bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是 bool 数组，访问以 "i=" 开头， 如：i=A[0]。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <param name="length">读取的数组长度信息</param>
    /// <returns>带有结果对象的结果数据</returns>
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        OperateResult<byte[]> read;
        if (length == 1 && !Regex.IsMatch(address, "\\[[0-9]+\\]$"))
        {
            read = await ReadAsync(address, length).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content));
        }
        read = await ReadAsync(address, length).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.Select((m) => m != 0).Take(length).ToArray());
    }

    /// <inheritdoc />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        CommunicationHelper.ExtractParameter(ref address, "type", 0);
        if (length == 1)
        {
            var read = await ReadWithTypeAsync([address], [length]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            return OperateResult.CreateSuccessResult(read.Content1);
        }

        var count = 0;
        var index = 0;
        var format = "[{0}]";
        var array = new List<byte>();
        var match = Regex.Match(address, "\\[[0-9]+\\]$");
        if (match.Success)
        {
            address = address.Remove(match.Index, match.Length);
            index = int.Parse(match.Value[1..^1]);
        }
        else
        {
            var match2 = Regex.Match(address, "\\[[0-9]+,[0-9]+\\]$");
            if (match2.Success)
            {
                address = address.Remove(match2.Index, match2.Length);
                var index2 = Regex.Matches(match2.Value, "[0-9]+")[1].Value;
                format = match2.Value.Replace(index2 + "]", "{0}]");
                index = int.Parse(index2);
            }
        }

        ushort dataType = 0;
        while (count < length)
        {
            if (count == 0)
            {
                var first = Math.Min(length, (ushort)248);
                var read = await ReadWithTypeAsync([address + string.Format(format, index)], [first]).ConfigureAwait(false);
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(read);
                }
                dataType = read.Content2;
                count += first;
                index += first;
                array.AddRange(read.Content1);
            }
            else
            {
                var len = (ushort)GetLengthFromRemain(dataType, length - count);
                var read = await ReadWithTypeAsync([address + string.Format(format, index)], [len]).ConfigureAwait(false);
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(read);
                }
                count += len;
                index += len;
                array.AddRange(read.Content1);
            }
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Read(System.String[],System.UInt16[])" />
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address, ushort[] length)
    {
        var read = await ReadWithTypeAsync(address, length).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content1);
    }

    /// <summary>
    /// 读取PLC的byte类型的数据。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <returns>带有结果对象的结果数据</returns>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }

    public async Task<OperateResult<ushort, byte[]>> ReadTagAsync(string address, ushort length = 1)
    {
        var read = await ReadWithTypeAsync([address], [length]).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content2, read.Content1);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteTagAsync(address, 209, value, !CommunicationHelper.IsAddressEndWithIndex(address) ? 1 : value.Length).ConfigureAwait(false);
    }

    public virtual async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
    {
        typeCode = (ushort)CommunicationHelper.ExtractParameter(ref address, "type", typeCode);
        var command = BuildWriteCommand(address, typeCode, value, length);
        if (!command.IsSuccess)
        {
            return command;
        }

        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
    }

    private OperateResult<string> ExtraStringContent(byte[] content, Encoding encoding)
    {
        try
        {
            if (content.Length >= 2)
            {
                int count = ByteTransform.TransUInt16(content, 0);
                return OperateResult.CreateSuccessResult(encoding.GetString(content, 2, count));
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(content));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>("Parse string failed: " + ex.Message + " Source: " + content.ToHexString(' '));
        }
    }

    /// <inheritdoc />
    public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return await ReadStringAsync(address, 1, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取字符串数据，默认为UTF-8编码.
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的string数据</returns>
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await ReadStringAsync(address, length, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        var read = await ReadAsync(address, length).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        return ExtraStringContent(read.Content, encoding);
    }

    public override async Task<OperateResult> WriteAsync(string address, short[] values)
    {
        return await WriteTagAsync(address, 195, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ushort[] values)
    {
        return await WriteTagAsync(address, 199, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteTagAsync(address, 196, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteTagAsync(address, 200, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteTagAsync(address, 202, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteTagAsync(address, 197, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteTagAsync(address, 201, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteTagAsync(address, 203, ByteTransform.TransByte(values), values.Length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await WriteAsync(address, value, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        var buffer = string.IsNullOrEmpty(value) ? [] : encoding.GetBytes(value);
        return await WriteTagAsync(address, 208, SoftBasic.SpliceArray(BitConverter.GetBytes((ushort)buffer.Length), buffer)).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await WriteTagAsync(address, 193, !value ? new byte[2] : [255, 255]).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await WriteTagAsync(address, 193, values.Select((m) => (byte)(m ? 1 : 0)).ToArray(), !CommunicationHelper.IsAddressEndWithIndex(address) ? 1 : values.Length).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteTagAsync(address, 194, [value]).ConfigureAwait(false);
    }

    public async Task<OperateResult<DateTime>> ReadDateAsync(string address)
    {
        return await AllenBradleyHelper.ReadDateAsync(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteDateAsync(this, address, date).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeAndDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteTimeAndDateAsync(this, address, date).ConfigureAwait(false);
    }

    public async Task<OperateResult<TimeSpan>> ReadTimeAsync(string address)
    {
        return await AllenBradleyHelper.ReadTimeAsync(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeAsync(string address, TimeSpan time)
    {
        return await AllenBradleyHelper.WriteTimeAsync(this, address, time).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeOfDateAsync(string address, TimeSpan timeOfDate)
    {
        return await AllenBradleyHelper.WriteTimeOfDateAsync(this, address, timeOfDate).ConfigureAwait(false);
    }

    private static byte[] GetAttributeAll()
    {
        return "00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes();
    }

    private OperateResult<byte[]> BuildReadCommand(string[] address, ushort[] length)
    {
        try
        {
            var list = new List<byte[]>();
            for (var i = 0; i < address.Length; i++)
            {
                list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i], isConnectedAddress: true));
            }
            return OperateResult.CreateSuccessResult(PackCommandService([.. list]));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    private OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
    {
        try
        {
            return OperateResult.CreateSuccessResult(PackCommandService(AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length, true)));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    public override string ToString()
    {
        return $"OmronConnectedCipNet[{IpAddress}:{Port}]";
    }
}
