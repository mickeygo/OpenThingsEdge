namespace ThingsEdge.Contracts.Codecs;

/// <summary>
/// 基于 HTTP 传输的 JSON 序列器。
/// </summary>
public interface IHttpJsonSerializer
{
    /// <summary>
    /// 将对象转换为指定类型的基元对象数组。
    /// 注意：对象必须为 JSON 反序列化的 <see cref="JsonElement"/> 对象。
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    T To<T>(object obj);

    /// <summary>
    /// 将对象转换为指定类型的基元对象。
    /// 注意：对象必须为 JSON 反序列化的 <see cref="JsonElement"/> 对象。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    T[] ToArray<T>(object obj);
}
