namespace ThingsEdge.Communication.Core;

/// <summary>
/// 一个用于多线程并发处理数据的模型类，适用于处理数据量非常庞大的情况
/// </summary>
/// <typeparam name="T">等待处理的数据类型</typeparam>
public sealed class SoftMultiTask<T>
{
    /// <summary>
    /// 一个双参数委托
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ex"></param>
    public delegate void MultiInfo(T item, Exception ex);

    /// <summary>
    /// 用于报告进度的委托，当finish等于count时，任务完成
    /// </summary>
    /// <param name="finish">已完成操作数量</param>
    /// <param name="count">总数量</param>
    /// <param name="success">成功数量</param>
    /// <param name="failed">失败数量</param>
    public delegate void MultiInfoTwo(int finish, int count, int success, int failed);

    /// <summary>
    /// 操作总数，判定操作是否完成
    /// </summary>
    private int _opCount;

    /// <summary>
    /// 判断是否所有的线程是否处理完成
    /// </summary>
    private int _opThreadCount = 1;

    /// <summary>
    /// 准备启动的处理数据的线程数量
    /// </summary>
    private readonly int _threadCount;

    /// <summary>
    /// 指示多线程处理是否在运行中，防止冗余调用
    /// </summary>
    private int _runStatus;

    /// <summary>
    /// 列表数据
    /// </summary>
    private readonly T[] _dataList;

    /// <summary>
    /// 需要操作的方法
    /// </summary>
    private readonly Func<T, bool> _operater;

    /// <summary>
    /// 已处理完成数量，无论是否异常
    /// </summary>
    private int _finishCount;

    /// <summary>
    /// 处理完成并实现操作数量
    /// </summary>
    private int _successCount;

    /// <summary>
    /// 处理过程中异常数量
    /// </summary>
    private int _failedCount;

    /// <summary>
    /// 用于触发事件的混合线程锁
    /// </summary>
    private readonly SimpleHybirdLock _hybirdLock = new();

    /// <summary>
    /// 指示处理状态是否为暂停状态
    /// </summary>
    private bool _isRunningStop;

    /// <summary>
    /// 指示系统是否需要强制退出
    /// </summary>
    private bool _isQuit;

    /// <summary>
    /// 在发生错误的时候是否强制退出后续的操作
    /// </summary>
    private bool _isQuitAfterException;

    /// <summary>
    /// 在发生错误的时候是否强制退出后续的操作
    /// </summary>
    public bool IsQuitAfterException
    {
        get
        {
            return _isQuitAfterException;
        }
        set
        {
            _isQuitAfterException = value;
        }
    }

    /// <summary>
    /// 异常发生时事件
    /// </summary>
    public event MultiInfo? OnExceptionOccur;

    /// <summary>
    /// 报告处理进度时发生
    /// </summary>
    public event MultiInfoTwo? OnReportProgress;

    /// <summary>
    /// 实例化一个数据处理对象
    /// </summary>
    /// <param name="dataList">数据处理列表</param>
    /// <param name="operater">数据操作方法，应该是相对耗时的任务</param>
    /// <param name="threadCount">需要使用的线程数</param>
    public SoftMultiTask(T[] dataList, Func<T, bool> operater, int threadCount = 10)
    {
        _dataList = dataList ?? throw new ArgumentNullException(nameof(dataList));
        _operater = operater ?? throw new ArgumentNullException(nameof(operater));
        if (threadCount < 1)
        {
            throw new ArgumentException("threadCount can not less than 1", nameof(threadCount));
        }
        _threadCount = threadCount;
        Interlocked.Add(ref _opCount, dataList.Length);
        Interlocked.Add(ref _opThreadCount, threadCount);
    }

    /// <summary>
    /// 启动多线程进行数据处理
    /// </summary>
    public void StartOperater()
    {
        if (Interlocked.CompareExchange(ref _runStatus, 0, 1) == 0)
        {
            for (var i = 0; i < _threadCount; i++)
            {
                var thread = new Thread(ThreadBackground)
                {
                    IsBackground = true
                };
                thread.Start();
            }
            JustEnded();
        }
    }

    /// <summary>
    /// 暂停当前的操作
    /// </summary>
    public void StopOperater()
    {
        if (_runStatus == 1)
        {
            _isRunningStop = true;
        }
    }

    /// <summary>
    /// 恢复暂停的操作
    /// </summary>
    public void ResumeOperater()
    {
        _isRunningStop = false;
    }

    /// <summary>
    /// 直接手动强制结束操作
    /// </summary>
    public void EndedOperater()
    {
        if (_runStatus == 1)
        {
            _isQuit = true;
        }
    }

    private void ThreadBackground()
    {
        while (true)
        {
            while (_isRunningStop)
            {
            }
            var num = Interlocked.Decrement(ref _opCount);
            if (num < 0)
            {
                break;
            }
            var val = _dataList[num];
            var flag2 = false;
            var flag3 = false;
            try
            {
                if (!_isQuit)
                {
                    flag2 = _operater(val);
                }
            }
            catch (Exception ex)
            {
                flag3 = true;
                OnExceptionOccur?.Invoke(val, ex);
                if (_isQuitAfterException)
                {
                    EndedOperater();
                }
            }
            finally
            {
                _hybirdLock.Enter();
                if (flag2)
                {
                    _successCount++;
                }
                if (flag3)
                {
                    _failedCount++;
                }
                _finishCount++;
                OnReportProgress?.Invoke(_finishCount, _dataList.Length, _successCount, _failedCount);
                _hybirdLock.Leave();
            }
        }
        JustEnded();
    }

    private void JustEnded()
    {
        if (Interlocked.Decrement(ref _opThreadCount) == 0)
        {
            _finishCount = 0;
            _failedCount = 0;
            _successCount = 0;
            Interlocked.Exchange(ref _opCount, _dataList.Length);
            Interlocked.Exchange(ref _opThreadCount, _threadCount + 1);
            Interlocked.Exchange(ref _runStatus, 0);
            _isRunningStop = false;
            _isQuit = false;
        }
    }
}
