using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Common;

/// <summary>
/// 一个简单的不持久化的序号自增类，采用线程安全实现，并允许指定最大数字，将包含该最大值，到达后清空从指定数开始。
/// </summary>
public sealed class SoftIncrementCount : IDisposable
{
    private long _start;

    private long _current;

    private long _max = long.MaxValue;

    private readonly SimpleHybirdLock _hybirdLock;

    private bool _disposedValue;

    /// <summary>
    /// 增加的单元，如果设置为0，就是不增加。如果为小于0，那就是减少，会变成负数的可能。
    /// </summary>
    public int IncreaseTick { get; set; } = 1;

    /// <summary>
    /// 获取当前的计数器的最大的设置值。<br />
    /// Get the maximum setting value of the current counter.
    /// </summary>
    public long MaxValue => _max;

    /// <summary>
    /// 实例化一个自增信息的对象，包括最大值，初始值，增量值。
    /// </summary>
    /// <param name="max">数据的最大值，必须指定</param>
    /// <param name="start">数据的起始值，默认为0</param>
    /// <param name="tick">每次的增量值</param>
    public SoftIncrementCount(long max, long start = 0L, int tick = 1)
    {
        _start = start;
        _max = max;
        _current = start;
        IncreaseTick = tick;
        _hybirdLock = new SimpleHybirdLock();
    }

    /// <summary>
    /// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为最小值，如果小于最小值，则会重置为最大值。
    /// </summary>
    /// <returns>计数自增后的值</returns>
    public long GetCurrentValue()
    {
        _hybirdLock.Enter();
        var num = _current;
        _current += IncreaseTick;
        if (_current > _max)
        {
            _current = _start;
        }
        else if (_current < _start)
        {
            _current = _max;
        }
        _hybirdLock.Leave();
        return num;
    }

    /// <summary>
    /// 重置当前序号的最大值，最大值应该大于初始值，如果当前值大于最大值，则当前值被重置为最大值。
    /// </summary>
    /// <param name="max">最大值</param>
    public void ResetMaxValue(long max)
    {
        _hybirdLock.Enter();
        if (max > _start)
        {
            if (max < _current)
            {
                _current = _start;
            }
            _max = max;
        }
        _hybirdLock.Leave();
    }

    /// <summary>
    /// 重置当前序号的初始值，需要小于最大值，如果当前值小于初始值，则当前值被重置为初始值。
    /// </summary>
    /// <param name="start">初始值</param>
    public void ResetStartValue(long start)
    {
        _hybirdLock.Enter();
        if (start < _max)
        {
            if (_current < start)
            {
                _current = start;
            }
            _start = start;
        }
        _hybirdLock.Leave();
    }

    /// <summary>
    /// 将当前的值重置为初始值。
    /// </summary>
    public void ResetCurrentValue()
    {
        _hybirdLock.Enter();
        _current = _start;
        _hybirdLock.Leave();
    }

    /// <summary>
    /// 将当前的值重置为指定值，该值不能大于max，如果大于max值，就会自动设置为max。
    /// </summary>
    /// <param name="value">指定的数据值</param>
    public void ResetCurrentValue(long value)
    {
        _hybirdLock.Enter();
        if (value > _max)
        {
            _current = _max;
        }
        else if (value < _start)
        {
            _current = _start;
        }
        else
        {
            _current = value;
        }
        _hybirdLock.Leave();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SoftIncrementCount[{_current}]";
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _hybirdLock.Dispose();
            }
            _disposedValue = true;
        }
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public void Dispose()
    {
        Dispose(disposing: true);
    }
}
