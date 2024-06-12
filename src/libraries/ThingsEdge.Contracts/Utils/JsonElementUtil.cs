namespace ThingsEdge.Contracts.Utils;

/// <summary>
/// JsonElement 帮助类。
/// </summary>
public static class JsonElementUtil
{
    /// <summary>
    /// 获取 <see cref="string"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, string? value, string? err) GetString(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.String)
        {
            return (false, default, "JsonElement 不为 String 类型。");
        }

        var v = jsonElement.GetString();
        return (true, v, default);
    }

    /// <summary>
    ///  获取 <see cref="bool"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, bool? value, string? err) GetBoolean(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.True)
        {
            return (true, true, default);
        }
        if (jsonElement.ValueKind == JsonValueKind.False)
        {
            return (true, false, default);
        }

        return (false, false, "值不能转换为 bool 类型。");
    }

    /// <summary>
    ///  获取 <see cref="byte"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, byte? value, string? err) GetByte(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetByte(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 Byte 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="ushort"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, ushort? value, string? err) GetUInt16(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetUInt16(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 UInt16 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="short"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, short? value, string? err) GetInt16(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetInt16(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 Int16 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="uint"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, uint? value, string? err) GetUInt32(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetUInt32(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 UInt32 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="int"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, int? value, string? err) GetInt32(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetInt32(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 Int32 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="float"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, float? value, string? err) GetFloat(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetSingle(out var v) ?
            (true, v, default) :
            (false, default, "格式化为 float 类型异常");
    }

    /// <summary>
    ///  获取 <see cref="double"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static (bool ok, double? value, string? err) GetDouble(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Number)
        {
            return (false, default, "JsonElement 不为 Number 类型。");
        }

        return jsonElement.TryGetDouble(out var v) ? 
            (true, v, default) : 
            (false, default, "格式化为 double 类型异常");
    }

    /// <summary>
    ///  获取 bool[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, bool[]? value, string? err) GetBooleanArray(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (singleAsMultiple)
            {
                if (jsonElement.ValueKind == JsonValueKind.True)
                {
                    return (true, [true], default);
                }
                if (jsonElement.ValueKind == JsonValueKind.False)
                {
                    return (true, [false], default);
                }
            }

            return (false, default, "JsonElement 不为 Array 类型。");
        }

        try
        {
            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetBoolean())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 byte[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, byte[]? value, string? err) GetByteArray(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetByte()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetByte())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 ushort[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, ushort[]? value, string? err) GetUInt16Array(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetUInt16()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetUInt16())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 short[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, short[]? value, string? err) GetInt16Array(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetInt16()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetInt16())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 uint[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, uint[]? value, string? err) GetUInt32Array(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetUInt32()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetUInt32())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 int[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, int[]? value, string? err) GetInt32Array(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetInt32()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetInt32())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 float[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, float[]? value, string? err) GetFloatArray(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetSingle()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetSingle())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 double[] 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="singleAsMultiple">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, double[]? value, string? err) GetDoubleArray(JsonElement jsonElement, bool singleAsMultiple = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (singleAsMultiple)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, [jsonElement.GetDouble()], default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            // 使用 JsonSerializer.Deserialize<double[]>(jsonElement.GetRawText()) 兼容性更强。
            var v = jsonElement.EnumerateArray()
                .Select(e => e.GetDouble())
                .ToArray();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }
}
