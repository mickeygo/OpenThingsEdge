using System.Text.Json.Nodes;

namespace ThingsEdge.Exchange.Contracts.Variables;

/// <summary>
/// 标记。
/// </summary>
public class Tag
{
    /// <summary>
    /// 全局唯一值。
    /// </summary>
    public string TagId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 标记名称
    /// </summary>
    [NotNull]
    public string? Name { get; init; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull, JsonRequired]
    public string? Address { get; init; }

    /// <summary>
    /// 数据长度。
    /// </summary>
    /// <remarks>注：只有当数据为字符串类型或是要定义为数组时，才需要指定长度。</remarks>
    public int Length { get; init; }

    /// <summary>
    /// 数据类型。
    /// </summary>
    [JsonRequired, JsonConverter(typeof(JsonStringEnumConverter))]
    public TagDataType DataType { get; init; }

    /// <summary>
    /// 扩展数据。
    /// </summary>
    /// <remarks>
    /// Tag 中只定义了必要属性，JSON 中其他属性在反序列化时会包含在扩展数据中，其中名称区分大小写。
    /// </remarks>
    [JsonExtensionData]
    public JsonObject? ExtraData { get; init; }

    /// <summary>
    /// 标记是否为数组对象。
    /// 当值不为 String 类型（包含 S7String 和 S7WString）且设定的长度大于 0 时，会判定为数组。
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return Length > 0
           && DataType is not (TagDataType.String or TagDataType.S7String or TagDataType.S7WString);
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
    /// <typeparam name="T">获取的值类型</typeparam>
    /// <param name="propertyName">JSON 属性名称</param>
    /// <returns></returns>
    public T? GetExtraValue<T>(string propertyName)
    {
        return ExtraData != null && ExtraData.TryGetPropertyValue(propertyName, out var value) && value is not null
            ? value.GetValue<T>()
            : default;
    }
}
