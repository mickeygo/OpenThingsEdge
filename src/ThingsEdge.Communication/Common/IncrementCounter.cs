namespace ThingsEdge.Communication.Common;

/// <summary>
/// 一个简单的不持久化的序号自增类，采用线程安全实现，并允许指定最大数字，将包含该最大值，到达后清空从指定数开始。
/// </summary>
public sealed class IncrementCounter
{
    private readonly long _start;
    private long _current;
    private readonly long _max;
    private readonly int _increaseTick = 1;

    /// <summary>
    /// 实例化一个自增信息的对象，包括最大值，初始值，增量值。
    /// </summary>
    /// <param name="max">数据的最大值，必须指定</param>
    /// <param name="start">数据的起始值，默认为 0</param>
    /// <param name="tick">每次的增量值，默认为 1</param>
    public IncrementCounter(long max, long start = 0, int tick = 1)
    {
        _start = start;
        _max = max;
        _current = start;
        _increaseTick = tick;
    }

    /// <summary>
    /// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为起始值。
    /// </summary>
    /// <returns>返回数据自增的前一个值</returns>
    public long OnNext()
    {
        var origin = _current;
        _current += _increaseTick;
        if (_current > _max)
        {
            _current = _start;
        }

        return origin;
    }

    /// <summary>
    /// 将当前的值重置为起始值。
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _current, _start);
    }

    public override string ToString()
    {
        return $"SoftIncrementCount[{_current}]";
    }
}
