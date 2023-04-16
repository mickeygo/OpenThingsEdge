namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// Switch 数据容器
/// </summary>
internal sealed class SwitchContainer
{
    private readonly ConcurrentDictionary<string, StreamWriterWrapper> _container = new();

    public SwitchContainer()
    {
    }

    /// <summary>
    /// 尝试获取对象。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <returns></returns>
    public bool TryGet(string key, [MaybeNullWhen(false)] out StreamWriterWrapper value)
    {
        value = default;
        if (_container.TryGetValue(key, out var writer))
        {
            value = writer;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取对象，若集合中没找到则创建。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <param name="path">文件路径。</param>
    /// <returns></returns>
    public StreamWriterWrapper GetOrCreate(string key, string path)
    {
        return _container.GetOrAdd(key, s =>
        {
            return new StreamWriterWrapper(path);
        });
    }

    /// <summary>
    /// 移除指定的对象，在成功移除后会一并释放对象资源。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    public void Remove(string key)
    {
        if (_container.TryRemove(key, out var writer))
        {
            writer.Dispose();
        }
    }

    /// <summary>
    /// 清空容器。
    /// </summary>
    public void Clear()
    {
        foreach (var writer in _container.Values)
        {
            writer.Dispose();
        }

        _container.Clear();
    }
}

/// <summary>
/// <see cref="StreamWriter"/> 包装类。
/// </summary>
public sealed class StreamWriterWrapper : IDisposable
{
    private readonly StreamWriter _writer;

    /// <summary>
    /// 写入的行数数量。
    /// </summary>
    public long LineCount { get; private set; }

    /// <summary>
    /// 当写入的行数到达设定的值时，会调用 Flush 将缓存数据刷入文件。默认为 128。
    /// </summary>
    public long FlushMaxLineCount { get; set; } = 128;

    /// <summary>
    /// 初始化一个新的对象。
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public StreamWriterWrapper(string path)
    {
        _writer = new(path, true);
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="value">要写入的数据。</param>
    /// <returns></returns>
    public async Task WriteAsync(string value)
    {
        ++LineCount;
        await _writer.WriteAsync(value);
        if (LineCount % FlushMaxLineCount == 0)
        {
            await _writer.FlushAsync();
        }
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <param name="value">要写入的数据。</param>
    /// <returns></returns>
    public async Task WriteLineAsync(string value)
    {
        ++LineCount;
        await _writer.WriteLineAsync(value);
        if (LineCount % FlushMaxLineCount == 0)
        {
            await _writer.FlushAsync();
        }
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
