using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC的CIP协议的类，支持NJ,NX,NY系列PLC，支持tag名的方式读写数据，假设你读取的是局部变量，那么使用 Program:MainProgram.变量名。
/// </summary>
public class OmronCipNet : AllenBradleyNet
{
    /// <summary>
    /// Specify the IP address and port to instantiate a communication object for a OmronCipNet PLC protocol
    /// </summary>
    /// <param name="ipAddress">PLC IpAddress</param>
    /// <param name="port">PLC Port</param>
    public OmronCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        if (length > 1)
        {
            return await ReadAsync([address], [1]).ConfigureAwait(false);
        }
        return await ReadAsync([address], [length]).ConfigureAwait(false);
    }

    public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, 0, length));
        }
        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, 0, length));
        }

        var startIndex = StringExtensions.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        var read = await ReadAsync(address, length).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }

        try
        {
            return OperateResult.CreateSuccessResult(encoding.GetString(count: ByteTransform.TransUInt16(read.Content, 0), bytes: read.Content, index: 2));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>("Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToHexString(' '));
        }
    }

    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var data = CollectionUtils.SpliceArray(new byte[2], CollectionUtils.ExpandToEvenLength(encoding.GetBytes(value)));
        data[0] = BitConverter.GetBytes(data.Length - 2)[0];
        data[1] = BitConverter.GetBytes(data.Length - 2)[1];
        return await WriteTagAsync(address, 208, data).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteTagAsync(address, 209, [value]).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"OmronCipNet[{IpAddress}:{Port}]";
    }
}
