using Ops.Communication.Profinet.Melsec;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// <see cref="MelsecMcNet"/> 扩展。
/// </summary>
public static class MelsecMcNetExtensions
{
    /// <summary>
    /// 三菱 MC 协议读取连续的地址。
    /// </summary>
    /// <remarks>
    /// <para>注：读取地址必须是连续的。</para>
    /// </remarks>
    /// <param name="melsecMcNet"></param>
    /// <param name="tags"></param>
    public static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadContinuationAsync(this MelsecMcNet melsecMcNet, IEnumerable<Tag> tags)
    {
        ushort length = 0;
        foreach (var tag in tags)
        {
            length += (ushort)CalTagWordLength(tag);
        }

        // 读取的长度为 word
        var result = await melsecMcNet.ReadAsync(tags.First().Address, length).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return (false, default, result.Message);
        }

        int index = 0;
        List<PayloadData> list = new(tags.Count());
        foreach (var tag in tags)
        {
            PayloadData tagPayload = PayloadData.FromTag(tag);

            // 非数组
            if (!tag.IsArray())
            {
                object data = tag.DataType switch
                {
                    TagDataType.Bit => melsecMcNet.ByteTransform.TransBool(result.Content, index),
                    TagDataType.Byte => melsecMcNet.ByteTransform.TransByte(result.Content, index),
                    TagDataType.Word => melsecMcNet.ByteTransform.TransUInt16(result.Content, index),
                    TagDataType.DWord => melsecMcNet.ByteTransform.TransUInt32(result.Content, index),
                    TagDataType.Int => melsecMcNet.ByteTransform.TransInt16(result.Content, index),
                    TagDataType.DInt => melsecMcNet.ByteTransform.TransInt32(result.Content, index),
                    TagDataType.Real => melsecMcNet.ByteTransform.TransSingle(result.Content, index),
                    TagDataType.LReal => melsecMcNet.ByteTransform.TransDouble(result.Content, index),
                    TagDataType.String => melsecMcNet.ByteTransform.TransString(result.Content, index, tag.Length, Encoding.ASCII).TrimEnd('\0'),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Value = data;
            }
            else
            {
                object data = tag.DataType switch
                {
                    TagDataType.Bit => melsecMcNet.ByteTransform.TransBool(result.Content, index, tag.Length),
                    TagDataType.Byte => melsecMcNet.ByteTransform.TransByte(result.Content, index, tag.Length),
                    TagDataType.Word => melsecMcNet.ByteTransform.TransUInt16(result.Content, index, tag.Length),
                    TagDataType.DWord => melsecMcNet.ByteTransform.TransUInt32(result.Content, index, tag.Length),
                    TagDataType.Int => melsecMcNet.ByteTransform.TransInt16(result.Content, index, tag.Length),
                    TagDataType.DInt => melsecMcNet.ByteTransform.TransInt32(result.Content, index, tag.Length),
                    TagDataType.Real => melsecMcNet.ByteTransform.TransSingle(result.Content, index, tag.Length),
                    TagDataType.LReal => melsecMcNet.ByteTransform.TransDouble(result.Content, index, tag.Length),
                    _ => throw new NotImplementedException(),
                };

                tagPayload.Value = data;
            }

            list.Add(tagPayload);
            index += CalTagByteLength(tag);
        }

        return (true, list, default);
    }

    private static int CalTagByteLength(Tag tag)
    {
        return tag.DataType switch
        {
            TagDataType.Bit or TagDataType.Byte or TagDataType.Word or TagDataType.Int => Len(tag.Length) << 1,
            TagDataType.DWord or TagDataType.DInt or TagDataType.Real => Len(tag.Length) << 2,
            TagDataType.LReal => Len(tag.Length) << 3,
            TagDataType.String => tag.Length << 1,
            _ => throw new NotImplementedException(),
        };
    }

    private static int CalTagWordLength(Tag tag)
    {
        return tag.DataType switch
        {
            TagDataType.Bit or TagDataType.Byte or TagDataType.Word or TagDataType.Int => Len(tag.Length),
            TagDataType.DWord or TagDataType.DInt or TagDataType.Real => Len(tag.Length) << 1,
            TagDataType.LReal => Len(tag.Length) << 2,
            TagDataType.String => tag.Length,
            _ => throw new NotImplementedException(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Len(int length)
    {
        return length == 0 ? 1 : length;
    }
}
