namespace ThingsEdge.Common.Queue;

/// <summary>
/// 后台任务队列接口
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 将工作事项加入后台任务。
    /// </summary>
    /// <param name="workItem">工作实现</param>
    /// <returns></returns>
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

    /// <summary>
    /// 从后台任务队列中取出任务。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
