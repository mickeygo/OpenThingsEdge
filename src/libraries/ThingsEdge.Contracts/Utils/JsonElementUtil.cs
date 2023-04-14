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

        try
        {
            var v = jsonElement.GetString();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<byte>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<ushort>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<short>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<uint>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<int>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<float>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
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

        try
        {
            var v = jsonElement.Deserialize<double>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="bool[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, bool[]? value, string? err) GetBooleanArray(JsonElement jsonElement, bool canInclude = false)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            if (canInclude)
            {
                if (jsonElement.ValueKind == JsonValueKind.True)
                {
                    return (true, new bool[] { true }, default);
                }
                if (jsonElement.ValueKind == JsonValueKind.False)
                {
                    return (true, new bool[] { false }, default);
                }
            }

            return (false, default, "JsonElement 不为 Array 类型。");
        }

        try
        {
            var v = jsonElement.Deserialize<bool[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="byte[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, byte[]? value, string? err) GetByteArray(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new byte[] { jsonElement.Deserialize<byte>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<byte[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="ushort[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, ushort[]? value, string? err) GetUInt16Array(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new ushort[] { jsonElement.Deserialize<ushort>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<ushort[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="short[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, short[]? value, string? err) GetInt16Array(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new short[] { jsonElement.Deserialize<short>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<short[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="uint[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, uint[]? value, string? err) GetUInt32Array(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new uint[] { jsonElement.Deserialize<uint>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<uint[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="int[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, int[]? value, string? err) GetInt32Array(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new int[] { jsonElement.Deserialize<int>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<int[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="float[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, float[]? value, string? err) GetFloatArray(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new float[] { jsonElement.Deserialize<float>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            var v = jsonElement.Deserialize<float[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    /// <summary>
    ///  获取 <see cref="double[]"/> 类型对象。
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="canInclude">是否单一值也以数组形式返回。</param>
    /// <returns></returns>
    public static (bool ok, double[]? value, string? err) GetDoubleArray(JsonElement jsonElement, bool canInclude = false)
    {
        try
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
            {
                if (canInclude)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        return (true, new double[] { jsonElement.Deserialize<double>() }, default);
                    }
                }

                return (false, default, "JsonElement 不为 Array 类型。");
            }

            // 使用 JsonSerializer.Deserialize<double[]>(jsonElement.GetRawText()) 兼容性更强。
            var v = jsonElement.Deserialize<double[]>();
            return (true, v, default);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }
}
