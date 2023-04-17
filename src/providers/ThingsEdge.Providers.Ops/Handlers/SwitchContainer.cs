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
    /// 从容器中移除指定的对象，在成功移除后会一并释放对象资源。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <param name="filepath">移除的文件路径。</param>
    /// <returns></returns>
    public bool TryRemove(string key, [MaybeNullWhen(false)] out string filepath)
    {
        filepath = default;
        if (_container.TryRemove(key, out var writer))
        {
            filepath = writer.FilePath;
            writer.Dispose();

            return true;
        }

        return false;
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
