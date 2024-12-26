using ThingsEdge.Exchange.Configuration;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线容器状态。
/// </summary>
/// <param name="Model">曲线模型</param>
/// <param name="Writer">曲线写入器</param>
internal sealed record CurveContainerState(CurveModel Model, ICurveWriter Writer);

/// <summary>
/// 曲线数据容器
/// </summary>
internal sealed class CurveContainer
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
    /// <param name="model">曲线模型</param>
    /// <param name="curveFileExt">文件类型</param>
    /// <param name="createPathFactory">创建文件路径工厂</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ICurveWriter GetOrCreate(string tagId, CurveModel model, CurveFileExt curveFileExt, Func<CurveModel, (string, string)> createPathFactory)
    {
        var state = _container.GetOrAdd(tagId, _ =>
        {
            var (path, relativePath) = createPathFactory(model);
            return curveFileExt switch
            {
                CurveFileExt.CSV => new CurveContainerState(model, new CsvCurveWriter(path, relativePath)),
                CurveFileExt.JSON => new CurveContainerState(model, new JsonCurveWriter(path, relativePath)),
                _ => throw new InvalidOperationException("曲线文件存储格式必须是 JSON 或 CSV"),
            };
        });
        return state.Writer;
    }

    /// <summary>
    /// 从容器中移除指定的对象，在成功移除后会一并释放对象资源。
    /// </summary>
    /// <param name="tagId">标记 Id。</param>
    /// <param name="removeTailCountBeforeSaving">保存前要移除的尾部行数，0 表示不移除</param>
    /// <returns></returns>
    public async Task<(bool ok, CurveContainerState? state)> SaveAndRemoveAsync(string tagId, int removeTailCountBeforeSaving = 0)
    {
        if (_container.TryRemove(tagId, out var state))
        {
            if (removeTailCountBeforeSaving > 0)
            {
                state.Writer.RemoveLineBody(removeTailCountBeforeSaving);
            }

            await state.Writer.SaveAsync().ConfigureAwait(false);
            state.Writer.Close();

            return (true, state);
        }

        return (false, default);
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
