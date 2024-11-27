namespace ThingsEdge.Exchange.Utils;

/// <summary>
/// JSON 对象帮助类。
/// </summary>
public static class JsonUtils
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip, // 允许注释
        AllowTrailingCommas = true, // 允许尾随逗号
        PropertyNameCaseInsensitive = true, // 属性名称匹配不区分大小写
    };

    /// <summary>
    /// JSON 对象反序列化。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">要反序列化的 JSON 文本。</param>
    /// <returns></returns>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, s_jsonSerializerOptions);
    }
}
