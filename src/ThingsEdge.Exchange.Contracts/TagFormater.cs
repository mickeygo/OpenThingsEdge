using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 标记数据格式化器。
/// </summary>
public static class TagFormater
{
    /// <summary>
    /// 格式数据。会将 object 对象转换为与 Tag 相应的数据类型。
    /// </summary>
    /// <param name="tag">数据对应的标记。</param>
    /// <param name="obj">要格式的数据。</param>
    /// <returns></returns>
    public static (bool ok, object? data, string? err) Format(Tag tag, object obj)
    {
        try
        {
            // 原始数据。
            object? obj2 = tag.DataType switch
            {
                TagDataType.Bit => tag.Length <= 1 ? Convert.ToBoolean(obj) : ConvertUtils.ToBooleanArray(obj),
                TagDataType.Byte => tag.Length <= 1 ? Convert.ToByte(obj) : ConvertUtils.ToByteArray(obj),
                TagDataType.Word => tag.Length <= 1 ? Convert.ToUInt16(obj) : ConvertUtils.ToUInt16Array(obj),
                TagDataType.DWord => tag.Length <= 1 ? Convert.ToUInt32(obj) : ConvertUtils.ToUInt32Array(obj),
                TagDataType.Int => tag.Length <= 1 ? Convert.ToInt16(obj) : ConvertUtils.ToInt16Array(obj),
                TagDataType.DInt => tag.Length <= 1 ? Convert.ToInt32(obj) : ConvertUtils.ToInt32Array(obj),
                TagDataType.Real => tag.Length <= 1 ? Convert.ToSingle(obj) : ConvertUtils.ToSingleArray(obj),
                TagDataType.LReal => tag.Length <= 1 ? Convert.ToDouble(obj) : ConvertUtils.ToDoubleArray(obj),
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
