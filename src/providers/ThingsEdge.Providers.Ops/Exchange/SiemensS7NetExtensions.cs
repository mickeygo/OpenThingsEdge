using Ops.Communication.Profinet.Siemens;
using ThingsEdge.Contracts.Devices;

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
    public static async Task<List<TagPayload>> ReadMultiAsync(this SiemensS7Net siemensS7Net, IEnumerable<Tag> tags)
    {
        int count = tags.Count();

        List<string> addresses = new(count);
        List<ushort> lengths = new(count);
        foreach (var tag in tags)
        {
            addresses.Add(tag.Address);
            lengths.Add((ushort)TagToByteLength(tag));
        }

        var result = await siemensS7Net.ReadAsync(addresses.ToArray(), lengths.ToArray()).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            // TODO: 处理批量读取出错信息。
            return new(0);
        }

        int index = 0;
        List<TagPayload> list = new(count);
        foreach (var tag in tags)
        {
            TagPayload tagPayload = new()
            {
                Payload = PayloadData.FromTag(tag),
            };

            // 非数组
            if (!tag.IsArray())
            {
                object data = tag.DataType switch
                {
                    DataType.Bit => siemensS7Net.ByteTransform.TransBool(result.Content, index),
                    DataType.Byte => siemensS7Net.ByteTransform.TransByte(result.Content, index),
                    DataType.Word => siemensS7Net.ByteTransform.TransUInt16(result.Content, index),
                    DataType.DWord => siemensS7Net.ByteTransform.TransUInt32(result.Content, index),
                    DataType.Int => siemensS7Net.ByteTransform.TransInt16(result.Content, index),
                    DataType.DInt => siemensS7Net.ByteTransform.TransInt32(result.Content, index),
                    DataType.Real => siemensS7Net.ByteTransform.TransSingle(result.Content, index),
                    DataType.LReal => siemensS7Net.ByteTransform.TransDouble(result.Content, index),
                    DataType.String or DataType.S7String => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length, Encoding.ASCII).TrimEnd('\0'),
                    DataType.S7WString => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length * 2, Encoding.Unicode).TrimEnd('\0'),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Payload.Value = data;
            }
            else
            {
                object data = tag.DataType switch
                {
                    DataType.Bit => siemensS7Net.ByteTransform.TransBool(result.Content, index, tag.Length),
                    DataType.Byte => siemensS7Net.ByteTransform.TransByte(result.Content, index, tag.Length),
                    DataType.Word => siemensS7Net.ByteTransform.TransUInt16(result.Content, index, tag.Length),
                    DataType.DWord => siemensS7Net.ByteTransform.TransUInt32(result.Content, index, tag.Length),
                    DataType.Int => siemensS7Net.ByteTransform.TransInt16(result.Content, index, tag.Length),
                    DataType.DInt => siemensS7Net.ByteTransform.TransInt32(result.Content, index, tag.Length),
                    DataType.Real => siemensS7Net.ByteTransform.TransSingle(result.Content, index, tag.Length),
                    DataType.LReal => siemensS7Net.ByteTransform.TransDouble(result.Content, index, tag.Length),
                    DataType.String or DataType.S7String => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length, Encoding.ASCII).TrimEnd('\0'),
                    DataType.S7WString => siemensS7Net.ByteTransform.TransString(result.Content, index + 2, tag.Length * 2, Encoding.Unicode).TrimEnd('\0'),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Payload.Value = data;

            }

            list.Add(tagPayload);
            index += TagToByteLength(tag);
        }

        return list;
    }

    private static bool TransBool(byte b, int index)
    {
        if (index < 1 || index > 8)
        {
            throw new InvalidOperationException();
        }

        int index0 = index - 1;
        return (b & 0x01 << index0) == 0x01 << index0;
    }

    private static int TagToByteLength(Tag tag)
    {
        return tag.DataType switch
        {
            DataType.Bit => tag.Length / 8 + (Len(tag.Length) % 8 != 0 ? 1 : 0),
            DataType.Byte => Len(tag.Length),
            DataType.Word or DataType.Int => Len(tag.Length) * 2,
            DataType.DWord or DataType.DInt or DataType.Real => Len(tag.Length) * 4,
            DataType.LReal => Len(tag.Length) * 8,
            DataType.String or DataType.S7String => tag.Length + 2,
            DataType.S7WString => tag.Length * 2 + 2,
            _ => throw new NotImplementedException(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Len(int length)
    {
        return length == 0 ? 1 : length;
    }
}
