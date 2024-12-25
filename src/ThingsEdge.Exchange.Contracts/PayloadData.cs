using System.Text.Json.Nodes;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 主体数据。
/// </summary>
public sealed class PayloadData
{
    /// <summary>
    /// 标记唯一Id。
    /// </summary>
    [NotNull]
    public string? TagId { get; init; }

    /// <summary>
    /// Tag 标记名称。
    /// </summary>
    [NotNull]
    public string? TagName { get; init; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull]
    public string? Address { get; init; }

    /// <summary>
    /// 数据长度。
    /// </summary>
    /// <remarks>注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。</remarks>
    public int Length { get; init; }

    /// <summary>
    /// 值，根据实际转换为对应的数据。
    /// </summary>
    [NotNull]
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; init; }

    /// <summary>
    /// Tag 的扩展数据。
    /// </summary>
    public JsonObject? ExtraData { get; init; }

    /// <summary>
    /// 标记是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return Length > 0
           && DataType is not (TagDataType.String or TagDataType.S7String or TagDataType.S7WString);
    }

    /// <summary>
    /// 提取扩展数据的值，不存在时则返回指定的默认值。
    /// </summary>
    /// <typeparam name="T">获取的值类型</typeparam>
    /// <param name="propertyName">JSON 属性名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public T GetExtraValue<T>(string propertyName, T defaultValue)
    {
        return GetExtraValue<T>(propertyName) ?? defaultValue;
    }

    /// <summary>
    /// 提取扩展数据的值，不存在时返回 null。
    /// </summary>
    /// <param name="propertyName">JSON 属性名称</param>
    /// <returns></returns>
    public string? GetExtraValue(string propertyName)
    {
        return GetExtraValue<string>(propertyName);
    }

    /// <summary>
    /// 提取扩展数据的值，不存在时返回 default。
    /// </summary>
    /// <typeparam name="T">JSON 属性名称</typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public T? GetExtraValue<T>(string propertyName)
    {
        return ExtraData != null && ExtraData.TryGetPropertyValue(propertyName, out var value) && value is not null
            ? value.GetValue<T>()
            : default;
    }

    /// <summary>
    /// 复制 Tag 数据到此对象。
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static PayloadData FromTag(Tag tag)
    {
        return new PayloadData
        {
            TagId = tag.TagId,
            TagName = tag.Name,
            Address = tag.Address,
            DataType = tag.DataType,
            Length = tag.Length,
            ExtraData = tag.ExtraData,
        };
    }
}
