namespace ThingsEdge.Contracts;

/// <summary>
/// 标记数据格式化器。
/// </summary>
public static class TagFormater
{
    /// <summary>
    /// 格式数据。会将 JsonElement 或 object 对象转换为与 Tag 相应的数据类型。
    /// </summary>
    /// <param name="tag">数据对应的标记。</param>
    /// <param name="obj">要格式的数据。</param>
    /// <returns></returns>
    public static (bool ok, object? data, string? err) Format(Tag tag, object obj)
    {
        // object 为 JSON 反序列的数据。
        if (obj is JsonElement jsonElement)
        {
            return tag.DataType switch
            {
                TagDataType.Bit => tag.Length <= 1 ? JsonElementUtil.GetBoolean(jsonElement) : JsonElementUtil.GetBooleanArray(jsonElement),
                TagDataType.Byte => tag.Length <= 1 ? JsonElementUtil.GetByte(jsonElement) : JsonElementUtil.GetByteArray(jsonElement),
                TagDataType.Word => tag.Length <= 1 ? JsonElementUtil.GetUInt16(jsonElement) : JsonElementUtil.GetUInt16Array(jsonElement),
                TagDataType.DWord => tag.Length <= 1 ? JsonElementUtil.GetUInt32(jsonElement) : JsonElementUtil.GetUInt32Array(jsonElement),
                TagDataType.Int => tag.Length <= 1 ? JsonElementUtil.GetInt16(jsonElement) : JsonElementUtil.GetInt16Array(jsonElement),
                TagDataType.DInt => tag.Length <= 1 ? JsonElementUtil.GetInt32(jsonElement) : JsonElementUtil.GetInt32Array(jsonElement),
                TagDataType.Real => tag.Length <= 1 ? JsonElementUtil.GetFloat(jsonElement) : JsonElementUtil.GetFloatArray(jsonElement),
                TagDataType.LReal => tag.Length <= 1 ? JsonElementUtil.GetDouble(jsonElement) : JsonElementUtil.GetDoubleArray(jsonElement),
                TagDataType.String or TagDataType.S7String or TagDataType.S7WString => JsonElementUtil.GetString(jsonElement),
                _ => (false, null, string.Empty),
            };
        }

        try
        {
            // 原始数据。
            object? obj2 = tag.DataType switch
            {
                TagDataType.Bit => tag.Length <= 1 ? Convert.ToBoolean(obj) : ObjectConvertUtil.ToBooleanArray(obj),
                TagDataType.Byte => tag.Length <= 1 ? Convert.ToByte(obj) : ObjectConvertUtil.ToByteArray(obj),
                TagDataType.Word => tag.Length <= 1 ? Convert.ToUInt16(obj) : ObjectConvertUtil.ToUInt16Array(obj),
                TagDataType.DWord => tag.Length <= 1 ? Convert.ToUInt32(obj) : ObjectConvertUtil.ToUInt32Array(obj),
                TagDataType.Int => tag.Length <= 1 ? Convert.ToInt16(obj) : ObjectConvertUtil.ToInt16Array(obj),
                TagDataType.DInt => tag.Length <= 1 ? Convert.ToInt32(obj) : ObjectConvertUtil.ToInt32Array(obj),
                TagDataType.Real => tag.Length <= 1 ? Convert.ToSingle(obj) : ObjectConvertUtil.ToSingleArray(obj),
                TagDataType.LReal => tag.Length <= 1 ? Convert.ToDouble(obj) : ObjectConvertUtil.ToDoubleArray(obj),
                TagDataType.String or TagDataType.S7String or TagDataType.S7WString => Convert.ToString(obj),
                _ => throw new NotImplementedException(),
            };

            return (true, obj2, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }
}
