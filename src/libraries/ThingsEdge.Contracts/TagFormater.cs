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
                DataType.Bit => tag.Length == 0 ? JsonElementUtil.GetBoolean(jsonElement) : JsonElementUtil.GetBooleanArray(jsonElement),
                DataType.Byte => tag.Length == 0 ? JsonElementUtil.GetByte(jsonElement) : JsonElementUtil.GetByteArray(jsonElement),
                DataType.Word => tag.Length == 0 ? JsonElementUtil.GetUInt16(jsonElement) : JsonElementUtil.GetUInt16Array(jsonElement),
                DataType.DWord => tag.Length == 0 ? JsonElementUtil.GetUInt32(jsonElement) : JsonElementUtil.GetUInt32Array(jsonElement),
                DataType.Int => tag.Length == 0 ? JsonElementUtil.GetInt16(jsonElement) : JsonElementUtil.GetInt16Array(jsonElement),
                DataType.DInt => tag.Length == 0 ? JsonElementUtil.GetInt32(jsonElement) : JsonElementUtil.GetInt32Array(jsonElement),
                DataType.Real => tag.Length == 0 ? JsonElementUtil.GetFloat(jsonElement) : JsonElementUtil.GetFloatArray(jsonElement),
                DataType.LReal => tag.Length == 0 ? JsonElementUtil.GetDouble(jsonElement) : JsonElementUtil.GetDoubleArray(jsonElement),
                DataType.String or DataType.S7String or DataType.S7WString => JsonElementUtil.GetString(jsonElement),
                _ => (false, null, ""),
            };
        }

        // 原始数据。
        object? obj2 = tag.DataType switch
        {
            DataType.Bit => Convert.ToBoolean(obj),
            DataType.Byte => Convert.ToByte(obj),
            DataType.Word => Convert.ToUInt16(obj),
            DataType.DWord => Convert.ToUInt32(obj),
            DataType.Int => Convert.ToInt16(obj),
            DataType.DInt => Convert.ToInt32(obj),
            DataType.Real => Convert.ToSingle(obj),
            DataType.LReal => Convert.ToDouble(obj),
            DataType.String or DataType.S7String or DataType.S7WString => Convert.ToString(obj),
            _ => throw new NotImplementedException(),
        };

        return (true, obj2, "");
    }
}
