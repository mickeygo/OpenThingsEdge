namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// <see cref="StreamWriter"/> 包装类。
/// </summary>
internal sealed class StreamWriterWrapper : IDisposable
{
    private readonly StreamWriter _sw;

    /// <summary>
    /// 已写入的次数。
    /// </summary>
    public long WrittenCount { get; private set; }

    /// <summary>
    /// 当写入的次数到达设定的值时，会调用 Flush 将缓存数据刷入文件。默认为 128。
    /// </summary>
    public long FlushWhenMaxWrittenCount { get; set; } = 128;

    /// <summary>
    /// 文件完整路径。
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// 初始化一个新的对象。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public StreamWriterWrapper(string path)
    {
        FilePath = path;
        _sw = new(path, true);
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="value">要写入的数据。</param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task WriteAsync(string value)
    {
        ++WrittenCount;
        await _sw.WriteAsync(value).ConfigureAwait(false);
        if (WrittenCount % FlushWhenMaxWrittenCount == 0)
        {
            await _sw.FlushAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="value">要写入的数据。</param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task WriteLineAsync(string value)
    {
        ++WrittenCount;
        await _sw.WriteLineAsync(value).ConfigureAwait(false);
        if (WrittenCount % FlushWhenMaxWrittenCount == 0)
        {
            await _sw.FlushAsync().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _sw.Dispose();
    }
}
