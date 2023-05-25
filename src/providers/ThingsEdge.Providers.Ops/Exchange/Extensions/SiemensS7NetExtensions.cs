using Ops.Communication.Profinet.Siemens;
using ThingsEdge.Contracts.Variables;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// <see cref="SiemensS7Net"/> 扩展。
/// </summary>
public static class SiemensS7NetExtensions
{
    /// <summary>
    /// S7 协议批量读取。
    /// </summary>
    /// <param name="siemensS7Net"></param>
    /// <param name="tags"></param>
    public static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(this SiemensS7Net siemensS7Net, IEnumerable<Tag> tags)
    {
        // 信息系统->对 PLC 进行编程->指令->通信->S7 通信->数据一致性:
        //  1. 对于 S7 通信指令“PUT”/“GET”，在编程和组态过程中必须考虑到一致性数据区域的大小。这是因为在目标设备（服务器）的用户程序中没有可以用于与用户程序进行通信数据同步的通信块。
        //  2. 对于 S7-300 和 C7-300（除CPU 318-2 DP 之外），在操作系统的循环控制点，在保持数据一致性的情况下将通信数据逐块（一块为 32 字节）复制到用户存储器中。
        //  对于大型数据区域，无法确保数据的一致性。如果要求达到规定的数据一致性，则用户程序中的通信数据不应超过 32 个字节（最多为 8 个字节，视具体版本而定）。
        //  3. 另一方面，在 S7-400 和 S7-1500 中，大小为 462 字节的块中的通信数据不在循环控制点处理，而是在程序循环期间的固定时间片内完成。
        //  变量的一致性则由系统保证。因此，使用指令 "PUT" / "GET" 或者在读/写变量（例如，由 OP 或 OS 读/写）时可以在保持一致性的情况下访问这些通信区。
        int allowMaxByte = siemensS7Net.CurrentPlc switch
        {
            SiemensPLCS.S1500 or SiemensPLCS.S400 => 462,
            SiemensPLCS.S1200 => 462,
            SiemensPLCS.S300 => 32,
            SiemensPLCS.S200 or SiemensPLCS.S200Smart => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        // 取最短的长度。
        allowMaxByte = Math.Min(allowMaxByte, siemensS7Net.PDULength);

        List<PayloadData> list = new(tags.Count());
        List<List<Tag>> matrix = new();
        int sum = 0, row = 0;
        foreach (var tag in tags)
        {
            if (matrix.Count == 0)
            {
                matrix.Add(new());
            }

            int n = TagToByteLength(tag);
            sum += n;
            if (sum > allowMaxByte)
            {
                row++;
                sum = n;

                matrix.Add(new());
            }

            matrix[row].Add(tag);
        }

        foreach (var tags2 in matrix)
        {
            var (ok, data, err) = await siemensS7Net.ReadPartMultiAsync(tags2).ConfigureAwait(false);
            if (!ok)
            {
                return (false, default, err);
            }

            list.AddRange(data!);
        }

        return (true, list, default);
    }

    private static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadPartMultiAsync(this SiemensS7Net siemensS7Net, IEnumerable<Tag> tags)
    {
        int count = tags.Count();

        List<string> addresses = new(count);
        List<ushort> lengths = new(count);
        foreach (var tag in tags)
        {
            addresses.Add(tag.Address);
            lengths.Add((ushort)TagToByteLength(tag)); // 数据长度
        }

        var result = await siemensS7Net.ReadAsync(addresses.ToArray(), lengths.ToArray()).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return (false, default, result.Message);
        }

        int index = 0;
        List<PayloadData> list = new(count);
        foreach (var tag in tags)
        {
            PayloadData tagPayload = PayloadData.FromTag(tag);

            // 非数组
            if (!tag.IsArray())
            {
                object data = tag.DataType switch
                {
                    TagDataType.Bit => siemensS7Net.ByteTransform.TransBool(result.Content, index),
                    TagDataType.Byte => siemensS7Net.ByteTransform.TransByte(result.Content, index),
                    TagDataType.Word => siemensS7Net.ByteTransform.TransUInt16(result.Content, index),
                    TagDataType.DWord => siemensS7Net.ByteTransform.TransUInt32(result.Content, index),
                    TagDataType.Int => siemensS7Net.ByteTransform.TransInt16(result.Content, index),
                    TagDataType.DInt => siemensS7Net.ByteTransform.TransInt32(result.Content, index),
                    TagDataType.Real => siemensS7Net.ByteTransform.TransSingle(result.Content, index),
                    TagDataType.LReal => siemensS7Net.ByteTransform.TransDouble(result.Content, index),
                    TagDataType.String or TagDataType.S7String => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length, Encoding.ASCII).TrimEnd('\0'),
                    TagDataType.S7WString => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length * 2, Encoding.Unicode).TrimEnd('\0'),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Value = data;
            }
            else
            {
                object data = tag.DataType switch
                {
                    TagDataType.Bit => siemensS7Net.ByteTransform.TransBool(result.Content, index, tag.Length),
                    TagDataType.Byte => siemensS7Net.ByteTransform.TransByte(result.Content, index, tag.Length),
                    TagDataType.Word => siemensS7Net.ByteTransform.TransUInt16(result.Content, index, tag.Length),
                    TagDataType.DWord => siemensS7Net.ByteTransform.TransUInt32(result.Content, index, tag.Length),
                    TagDataType.Int => siemensS7Net.ByteTransform.TransInt16(result.Content, index, tag.Length),
                    TagDataType.DInt => siemensS7Net.ByteTransform.TransInt32(result.Content, index, tag.Length),
                    TagDataType.Real => siemensS7Net.ByteTransform.TransSingle(result.Content, index, tag.Length),
                    TagDataType.LReal => siemensS7Net.ByteTransform.TransDouble(result.Content, index, tag.Length),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Value = data;
            }

            list.Add(tagPayload);
            index += TagToByteLength(tag);
        }

        return (true, list, default);
    }

    //private static bool TransBool(byte b, int index)
    //{
    //    if (index < 1 || index > 8)
    //    {
    //        throw new InvalidOperationException();
    //    }

    //    int index0 = index - 1;
    //    return (b & 0x01 << index0) == 0x01 << index0;
    //}

    private static int TagToByteLength(Tag tag)
    {
        return tag.DataType switch
        {
            TagDataType.Bit => tag.Length / 8 + (Len(tag.Length) % 8 != 0 ? 1 : 0), // tag.Length >> 3
            TagDataType.Byte => Len(tag.Length),
            TagDataType.Word or TagDataType.Int => Len(tag.Length) * 2,
            TagDataType.DWord or TagDataType.DInt or TagDataType.Real => Len(tag.Length) * 4,
            TagDataType.LReal => Len(tag.Length) * 8,
            TagDataType.String or TagDataType.S7String => tag.Length + 2,
            TagDataType.S7WString => tag.Length * 2 + 2,
            _ => throw new NotImplementedException(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Len(int length)
    {
        return length == 0 ? 1 : length;
    }
}
