using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙PLC的CIP协议的类，支持NJ,NX,NY系列PLC，支持tag名的方式读写数据，假设你读取的是局部变量，那么使用 Program:MainProgram.变量名。
/// </summary>
public class OmronCipNet : AllenBradleyNet
{
    /// <summary>
    /// Instantiate a communication object for a OmronCipNet PLC protocol
    /// </summary>
    public OmronCipNet()
    {
    }

    /// <summary>
    /// Specify the IP address and port to instantiate a communication object for a OmronCipNet PLC protocol
    /// </summary>
    /// <param name="ipAddress">PLC IpAddress</param>
    /// <param name="port">PLC Port</param>
    public OmronCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        if (length > 1)
        {
            return Read([address], [1]);
        }
        return Read([address], [length]);
    }

    /// <inheritdoc />
    public override OperateResult<short[]> ReadInt16(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt16(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<ushort[]> ReadUInt16(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt16(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<int[]> ReadInt32(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt32(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt32(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<float[]> ReadFloat(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransSingle(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransSingle(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<long[]> ReadInt64(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt64(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt64(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransUInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<double[]> ReadDouble(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransDouble(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(Read(address, 1), (m) => ByteTransform.TransDouble(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
    {
        var operateResult = Read(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        try
        {
            int count = ByteTransform.TransUInt16(operateResult.Content, 0);
            return OperateResult.CreateSuccessResult(encoding.GetString(operateResult.Content, 2, count));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>("Parse failed: " + ex.Message + Environment.NewLine + "Source: " + operateResult.Content.ToHexString(' '));
        }
    }

    /// <inheritdoc />
    protected override int GetWriteValueLength(string address, int length)
    {
        return !Regex.IsMatch(address, "\\[[0-9]+\\]$") ? 1 : length;
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var array = SoftBasic.SpliceArray(new byte[2], SoftBasic.ArrayExpandToLengthEven(encoding.GetBytes(value)));
        array[0] = BitConverter.GetBytes(array.Length - 2)[0];
        array[1] = BitConverter.GetBytes(array.Length - 2)[1];
        return WriteTag(address, 208, array);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, byte value)
    {
        return WriteTag(address, 209, [value]);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        if (length > 1)
        {
            return await ReadAsync([address], [1]).ConfigureAwait(false);
        }
        return await ReadAsync([address], [length]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, startIndex >= 0 ? startIndex * 2 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, startIndex >= 0 ? startIndex * 4 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        if (length == 1)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, 0, length));
        }
        var startIndex = CommunicationHelper.ExtractStartIndex(ref address);
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 1).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, startIndex >= 0 ? startIndex * 8 : 0, length));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var data = SoftBasic.SpliceArray(new byte[2], SoftBasic.ArrayExpandToLengthEven(encoding.GetBytes(value)));
        data[0] = BitConverter.GetBytes(data.Length - 2)[0];
        data[1] = BitConverter.GetBytes(data.Length - 2)[1];
        return await WriteTagAsync(address, 208, data).ConfigureAwait(false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronCipNet.Write(System.String,System.Byte)" />
    public override async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteTagAsync(address, 209, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronCipNet[{IpAddress}:{Port}]";
    }
}
