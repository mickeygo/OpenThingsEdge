namespace ThingsEdge.Providers.Ops.Handlers.Curve;

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
    long WrittenCount { get; }

    /// <summary>
    /// 保存文件路径
    /// </summary>
    string FilePath { get; init; }

    /// <summary>
    /// 写入头信息
    /// </summary>
    /// <param name="header">头命名集合</param>
    /// <returns></returns>
    void WriteHeader(IEnumerable<string> header);

    /// <summary>
    /// 写入一行主体数据
    /// </summary>
    /// <param name="item">主体信息集合</param>
    /// <returns></returns>
    void WriteLineBody(IEnumerable<PayloadData> item);

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
