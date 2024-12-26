using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线数据写入器
/// </summary>
internal interface ICurveWriter
{
    /// <summary>
    /// 写入器是否已关闭。
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// 已写入的数量。
    /// </summary>
    int WrittenCount { get; }

    /// <summary>
    /// 保存文件的绝对路径
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// 保存文件的绝对路径
    /// </summary>
    string RelativePath { get; }

    /// <summary>
    /// 写入头信息
    /// </summary>
    /// <param name="header">头命名集合</param>
    /// <returns></returns>
    void WriteHeader(IEnumerable<string> header);

    /// <summary>
    /// 写入一行主体数据
    /// </summary>
    /// <param name="items">主体信息集合</param>
    /// <returns></returns>
    void WriteLineBody(IEnumerable<PayloadData> items);

    /// <summary>
    /// 移除尾部指定数量的数据。
    /// </summary>
    /// <param name="count">要移除的数量，当超过总数时会情况</param>
    void RemoveLineBody(int count);

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <exception cref="IOException"></exception>
    Task SaveAsync();

    /// <summary>
    /// 关闭写入器
    /// </summary>
    void Close();
}
