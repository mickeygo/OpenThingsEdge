using ThingsEdge.Communication.Profinet.Siemens;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Connectors.Drivers;

/// <summary>
/// <see cref="SiemensS7Net"/> 扩展。
/// </summary>
internal static class SiemensS7NetExtensions
{
    /// <summary>
    /// S7 协议批量读取。
    /// </summary>
    /// <param name="siemensS7Net"></param>
    /// <param name="tags">要批量读取的标记集合</param>
    /// <param name="maxPDUSize">允许最大的 PDU 长度，为 0 时采用全局设定值。</param>
    public static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(this SiemensS7Net siemensS7Net, IEnumerable<Tag> tags, int maxPDUSize = 0)
    {
        // 取最短的长度。
        var allowMaxByte = maxPDUSize > 0 ? Math.Min(maxPDUSize, siemensS7Net.PDULength) : siemensS7Net.PDULength;

        List<PayloadData> list = new(tags.Count());
        List<List<Tag>> matrix = [];
        int sum = 0, row = 0;
        foreach (var tag in tags)
        {
            if (matrix.Count == 0)
            {
                matrix.Add([]);
            }

            var n = CalTagByteLength(tag);
            sum += n;
            // 总长度超过 PDU 时，矩阵新增一行，写入数据。
            if (sum > allowMaxByte)
            {
                row++;
                sum = n;

                matrix.Add([]);
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
        var count = tags.Count();

        List<string> addresses = new(count);
        List<ushort> lengths = new(count);
        foreach (var tag in tags)
        {
            addresses.Add(tag.Address);
            lengths.Add((ushort)CalTagByteLength(tag)); // 数据长度
        }

        var result = await siemensS7Net.ReadAsync([.. addresses], [.. lengths]).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return (false, default, result.Message);
        }

        var index = 0;
        List<PayloadData> list = new(count);
        foreach (var tag in tags)
        {
            var tagPayload = PayloadData.FromTag(tag);

            // 非数组
            if (!tag.IsArray())
            {
                // .NET 中 byte 是无符号类型，sbyte 是有符号类型。
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
                    TagDataType.String or TagDataType.S7String => result.Content[index + 1] > 0
                        ? siemensS7Net.ByteTransform.TransString(result.Content, index + 2, result.Content[index + 1], Encoding.ASCII)
                        : string.Empty,
                    TagDataType.S7WString => result.Content[2] + result.Content[3] > 0
                        ? siemensS7Net.ByteTransform.TransString(result.Content, index + 4, (result.Content[2] * 256 + result.Content[3]) * 2, Encoding.Unicode)
                        : string.Empty,
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
            index += CalTagByteLength(tag);
        }

        return (true, list, default);
    }

    /// <summary>
    /// 计算 Tag 所占的字节长度。
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static int CalTagByteLength(Tag tag)
    {
        return tag.DataType switch
        {
            TagDataType.Bit => tag.Length / 8 + (Len(tag.Length) % 8 != 0 ? 1 : 0), // tag.Length >> 3
            TagDataType.Byte => Len(tag.Length),
            TagDataType.Word or TagDataType.Int => Len(tag.Length) * 2,
            TagDataType.DWord or TagDataType.DInt or TagDataType.Real => Len(tag.Length) * 4,
            TagDataType.LReal => Len(tag.Length) * 8,
            TagDataType.String or TagDataType.S7String => tag.Length + 2,
            TagDataType.S7WString => tag.Length * 2 + 4,
            _ => throw new NotImplementedException(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Len(int length)
    {
        return length == 0 ? 1 : length;
    }
}
