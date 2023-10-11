namespace ThingsEdge.Contracts;

/// <summary>
/// <see cref="PayloadData"/> 基于 System.Text.Json.<see cref="JsonElement"/> 类型提取数据的扩展对象。
/// </summary>
public static class PayloadDataExtensions2
{
    /// <summary>
    /// 将对象转换为 <see cref="string"/> 类型。
    /// 若对象是数组或是数字，会返回其序列化中的原始文本。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public static string FetchString(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => jsonElement.GetString() ?? "",
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number or JsonValueKind.Array => jsonElement.GetRawText(),
            JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Null => jsonElement.GetRawText(),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// 将对象转换为 <see cref="bool"/> 值。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static bool FetchBoolean(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetBoolean(jsonElement);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data!.Value;
    }

    /// <summary>
    /// 将对象转换为 <see cref="byte"/> 值。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static byte FetchByte(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetByte(jsonElement);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data!.Value;
    }

    /// <summary>
    /// 将对象转换为 <see cref="int"/> 类型。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static int FetchInt(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetInt32(jsonElement);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data!.Value;
    }

    /// <summary>
    /// 将对象转换为 <see cref="double"/> 类型。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static double FetchDouble(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetDouble(jsonElement);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data!.Value;
    }

    /// <summary>
    /// 将对象转换为 <see cref="bool[]"/> 数组。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static bool[] FetchBooleanArray(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetBooleanArray(jsonElement, true);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data ?? Array.Empty<bool>();
    }

    /// <summary>
    /// 将对象转换为 <see cref="byte[]"/> 数组。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static byte[] FetchByteArray(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetByteArray(jsonElement, true);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data ?? Array.Empty<byte>();
    }

    /// <summary>
    /// 将对象转换为 <see cref="int[]"/> 数组。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static int[] FetchIntArray(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetInt32Array(jsonElement, true);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data ?? Array.Empty<int>();
    }

    /// <summary>
    /// 将对象转换为 <see cref="double[]"/> 数组。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>用于 System.Text.Json.<see cref="JsonElement"/> 序列后解析对象。</remarks>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public static double[] FetchDoubleArray(this PayloadData payload)
    {
        if (payload.Value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        var (ok, data, err) = JsonElementUtil.GetDoubleArray(jsonElement, true);
        if (!ok)
        {
            throw new InvalidOperationException(err);
        }

        return data ?? Array.Empty<double>();
    }
}
