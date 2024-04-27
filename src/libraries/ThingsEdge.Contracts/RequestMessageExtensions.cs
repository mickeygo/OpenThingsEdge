namespace ThingsEdge.Contracts;

/// <summary>
/// <see cref="RequestMessage"/> 对象扩展。
/// </summary>
public static class RequestMessageExtensions
{
    /// <summary>
    /// 获取对象值，若没找标记，则返回 null。
    /// 若对象是数组或是数字，会返回其序列化中的原始文本。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string? FetchString(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => jsonElement.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number or JsonValueKind.Array => jsonElement.GetRawText(),
            JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Null => jsonElement.GetRawText(),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// 获取 <see cref="bool"/> 值，若没找到，返回 null。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool? FetchBoolean(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        if (jsonElement.ValueKind == JsonValueKind.False)
        {
            return true;
        }

        throw new InvalidOperationException("JsonElement 不为 True 或 False 类型。");
    }

    /// <summary>
    /// 获取 <see cref="byte"/> 值，若没找到，返回 null。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte? FetchByte(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }

        return jsonElement.Deserialize<byte>();
    }

    /// <summary>
    /// 获取 <see cref="int"/> 值，若没找到，返回 null。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int? FetchInt(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }
       
        return jsonElement.Deserialize<int>();
    }

    /// <summary>
    /// 获取 <see cref="double"/> 值，若没找到，返回 null。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static double? FetchDouble(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }

        return jsonElement.Deserialize<double>();
    }

    /// <summary>
    /// 获取 <see cref="bool"/> 数组，若没找到，返回 null。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool[]? FetchBooleanArray(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (jsonElement.ValueKind == JsonValueKind.True)
            {
                return [true];
            }
            if (jsonElement.ValueKind == JsonValueKind.False)
            {
                return [false];
            }

            throw new InvalidOperationException("JsonElement 不为 True 或 False 类型。");
        }

        return jsonElement.Deserialize<bool[]>();
    }

    /// <summary>
    /// 获取 <see cref="byte"/> 数组，若没找到，返回 null。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte[]? FetchByteArray(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return [jsonElement.Deserialize<byte>()];
            }

            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }

        return jsonElement.Deserialize<byte[]>();
    }

    /// <summary>
    /// 获取 <see cref="int"/> 数组，若没找到，返回 null。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int[]? FetchIntArray(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return [jsonElement.Deserialize<int>()];
            }

            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }

        return jsonElement.Deserialize<int[]>();
    }

    /// <summary>
    /// 获取 <see cref="double"/> 数组，若没找到，返回 null。
    /// 若对象是单一值，会转成含有一个值的数组。
    /// </summary>
    /// <remarks>获取的值必须为 System.Text.Json.<see cref="JsonElement"/> 类型，否则会抛出异常。</remarks>
    /// <param name="requestMessage"></param>
    /// <param name="tagName">标记名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static double[]? FetchDoubleArray(this RequestMessage requestMessage, string tagName)
    {
        var value = requestMessage.GetData(tagName)?.Value;
        if (value is null)
        {
            return default;
        }

        if (value is not JsonElement jsonElement)
        {
            throw new InvalidOperationException("不能将值转换为 JsonElement 类型对象。");
        }

        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return [jsonElement.Deserialize<double>()];
            }

            throw new InvalidOperationException("JsonElement 不为 Number 类型。");
        }

        return jsonElement.Deserialize<double[]>();
    }
}
