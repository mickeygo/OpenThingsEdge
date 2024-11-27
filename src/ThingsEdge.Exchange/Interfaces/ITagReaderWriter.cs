using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Interfaces;

/// <summary>
/// 标记数据读写接口。
/// </summary>
public interface ITagReaderWriter
{
    /// <summary>
    /// 读取标记配置中指定标记的数据。
    /// </summary>
    /// <param name="tagId">要读取数据的标记 Id。</param>
    /// <returns></returns>
    Task<(bool ok, PayloadData? data, string? err)> ReadAsync(string tagId);

    /// <summary>
    /// 批量读取标记配置中指定标记的数据。
    /// </summary>
    /// <param name="tagIds">要写入数据的标记 Id 集合，多个 tag 应该来源于一个 Device。</param>
    /// <param name="mulitple">是否允许一次性批量读取。部分设备支持批量读取，但要注意可能需要使用连续地址。</param>
    /// <returns></returns>
    Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(string[] tagIds, bool mulitple = true);

    /// <summary>
    /// 向标记配置中指定标记写入数据。
    /// </summary>
    /// <param name="tagId">要写入数据的标记 Id。</param>
    /// <param name="data">要写入的数据。</param>
    /// <returns></returns>
    Task<(bool ok, string? err)> WriteAsync(string tagId, object data);
}
