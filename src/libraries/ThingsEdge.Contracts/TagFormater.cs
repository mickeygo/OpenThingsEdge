using ThingsEdge.Contracts.Devices;
using ThingsEdge.Contracts.Utils;

namespace ThingsEdge.Contracts;

/// <summary>
/// 标记数据格式化器。
/// </summary>
public static class TagFormater
{
    /// <summary>
    /// 格式数据。
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
                TagDataType.Bit => tag.Length == 0 ? JsonElementUtil.GetBoolean(jsonElement) : JsonElementUtil.GetBooleanArray(jsonElement),
                TagDataType.Byte => tag.Length == 0 ? JsonElementUtil.GetByte(jsonElement) : JsonElementUtil.GetByteArray(jsonElement),
                TagDataType.Word => tag.Length == 0 ? JsonElementUtil.GetUInt16(jsonElement) : JsonElementUtil.GetUInt16Array(jsonElement),
                TagDataType.DWord => tag.Length == 0 ? JsonElementUtil.GetUInt32(jsonElement) : JsonElementUtil.GetUInt32Array(jsonElement),
                TagDataType.Int => tag.Length == 0 ? JsonElementUtil.GetInt16(jsonElement) : JsonElementUtil.GetInt16Array(jsonElement),
                TagDataType.DInt => tag.Length == 0 ? JsonElementUtil.GetInt32(jsonElement) : JsonElementUtil.GetInt32Array(jsonElement),
                TagDataType.Real => tag.Length == 0 ? JsonElementUtil.GetFloat(jsonElement) : JsonElementUtil.GetFloatArray(jsonElement),
                TagDataType.LReal => tag.Length == 0 ? JsonElementUtil.GetDouble(jsonElement) : JsonElementUtil.GetDoubleArray(jsonElement),
                TagDataType.String or TagDataType.S7String or TagDataType.S7WString => JsonElementUtil.GetString(jsonElement),
                _ => (false, null, ""),
            };
        }

        // 原始数据。
        object? obj2 = tag.DataType switch
        {
            TagDataType.Bit => Convert.ToBoolean(obj),
            TagDataType.Byte => Convert.ToByte(obj),
            TagDataType.Word => Convert.ToUInt16(obj),
            TagDataType.DWord => Convert.ToUInt32(obj),
            TagDataType.Int => Convert.ToInt16(obj),
            TagDataType.DInt => Convert.ToInt32(obj),
            TagDataType.Real => Convert.ToSingle(obj),
            TagDataType.LReal => Convert.ToDouble(obj),
            TagDataType.String or TagDataType.S7String or TagDataType.S7WString => Convert.ToString(obj),
            _ => throw new NotImplementedException(),
        };

        return (true, obj2, "");
    }
}
