namespace ThingsEdge.Router.Model;

/// <summary>
/// 标记数据监控器。
/// </summary>
public sealed class TagDataMonitor
{
    private readonly TagDataSet _dataSet = new();
    private long _lastChecked;

    /// <summary>
    /// 设置标记对应的数据。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <param name="data">要设置的数据</param>
    public void Set(string tagId, PayloadData data)
    {
        _dataSet.Set(tagId, data);
    }

    // <summary>
    /// 获取标记对应的数据，没有找到标记则返回 null。
    /// </summary>
    /// <param name="tagId">标记Id</param>
    /// <returns></returns>
    public TagData? Get(string tagId)
    {
        return _dataSet.Get(tagId);
    }

    /// <summary>
    /// 距离离上次检测的时间段内是否有更新数据。
    /// </summary>
    /// <returns></returns>
    public bool HasUpdatedFromLast()
    {
        if (_lastChecked == _dataSet.Version)
        {
            return false;
        }

        _lastChecked = _dataSet.Version;

        return true;
    }
}
