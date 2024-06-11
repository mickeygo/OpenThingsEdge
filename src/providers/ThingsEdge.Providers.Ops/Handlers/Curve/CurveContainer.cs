using ThingsEdge.Providers.Ops.Configuration;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 曲线容器状态
/// </summary>
/// <param name="Model">曲线模型</param>
/// <param name="Writer">曲线写入器</param>
internal sealed record class CurveContainerState(CurveModel Model, ICurveWriter Writer);

/// <summary>
/// 曲线数据容器
/// </summary>
internal sealed class CurveContainer : ISingletonDependency
{
    private readonly ConcurrentDictionary<string, CurveContainerState> _container = new();

    /// <summary>
    /// 容器中是否包含指定的对象。
    /// </summary>
    /// <param name="tagId">标记 Id。</param>
    /// <returns></returns>
    public bool Contains(string tagId)
    {
        return _container.ContainsKey(tagId);
    }

    /// <summary>
    /// 尝试获取对象。
    /// </summary>
    /// <param name="tagId">标记 Id。</param>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool TryGet(string tagId, [MaybeNullWhen(false)] out CurveContainerState state)
    {
        state = default;
        if (_container.TryGetValue(tagId, out var state2))
        {
            state = state2;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取对象，若集合中没找到则创建。
    /// </summary>
    /// <param name="tagId">标记 Id。</param>
    /// <param name="path">文件路径。</param>
    /// <param name="model">曲线模型</param>
    /// <param name="curveFileExt">文件类型</param>
    /// <returns></returns>
    public ICurveWriter GetOrCreate(string tagId, string path, CurveModel model, CurveFileExt curveFileExt)
    {
        var state = _container.GetOrAdd(tagId, _ => curveFileExt switch
            {
                CurveFileExt.CSV => new CurveContainerState(model, new CsvCurveWriter(path)),
                CurveFileExt.JSON => new CurveContainerState(model, new JsonCurveWriter(path)),
                _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
            });
        return state.Writer;
    }

    /// <summary>
    /// 从容器中移除指定的对象，在成功移除后会一并释放对象资源。
    /// </summary>
    /// <param name="key">对象唯一值。</param>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool TrySaveAndRemove(string key, [MaybeNullWhen(false)] out CurveContainerState state)
    {
        state = default;
        if (_container.TryRemove(key, out var state2))
        {
            state = state2;
            state.Writer.SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            state.Writer.Close();

            return true;
        }

        return false;
    }

    /// <summary>
    /// 清空容器。
    /// </summary>
    public void Clear()
    {
        foreach (var state in _container.Values)
        {
            state.Writer.Close();
        }

        _container.Clear();
    }
}
