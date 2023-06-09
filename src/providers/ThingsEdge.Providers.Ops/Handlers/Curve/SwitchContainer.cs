﻿using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// Switch 数据容器
/// </summary>
internal sealed class SwitchContainer : ISingletonDependency
{
    private readonly ConcurrentDictionary<string, ICurveWriter> _container = new();
    private readonly CurveConfig _curveConfig;

    public SwitchContainer(IOptionsMonitor<OpsConfig> opsConfig)
    {
        _curveConfig = opsConfig.CurrentValue.Curve;
    }

    /// <summary>
    /// 容器中是否包含指定的对象。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <returns></returns>
    public bool Contains(string key)
    {
        return _container.ContainsKey(key);
    }

    /// <summary>
    /// 尝试获取对象。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <returns></returns>
    public bool TryGet(string key, [MaybeNullWhen(false)] out ICurveWriter value)
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
    public ICurveWriter GetOrCreate(string key, string path)
    {
        return _container.GetOrAdd(key, s =>
        {
            return _curveConfig.FileExt switch
            {
                CurveFileExt.CSV => new CsvCurveWriter(path),
                CurveFileExt.JSON => new JsonCurveWriter(path),
                _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
            };
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
            writer.SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            writer.Close();

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
            writer.Close();
        }

        _container.Clear();
    }
}
