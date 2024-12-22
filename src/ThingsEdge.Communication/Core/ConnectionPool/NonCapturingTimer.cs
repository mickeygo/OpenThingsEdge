namespace ThingsEdge.Communication.Core.ConnectionPool;

/// <summary>
/// 基于 <see cref="Timer"/> 的不捕获上下文的定时器。
/// </summary>
internal static class NonCapturingTimer
{
    public static Timer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
    {
        ArgumentNullException.ThrowIfNull(callback);

        // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
        var restoreFlow = false;
        try
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow(); // 在异步线程间取消执行上下文的流动。
                restoreFlow = true;
            }

            return new Timer(callback, state, dueTime, period);
        }
        finally
        {
            // Restore the current ExecutionContext
            if (restoreFlow)
            {
                ExecutionContext.RestoreFlow(); // 在异步线程间恢复执行上下文的流动。
            }
        }
    }
}
