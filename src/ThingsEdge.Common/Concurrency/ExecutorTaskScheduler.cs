namespace ThingsEdge.Common.Concurrency;

public sealed class ExecutorTaskScheduler : TaskScheduler
{
    protected override IEnumerable<Task>? GetScheduledTasks() => null;

    protected override void QueueTask(Task task)
    {
        throw new NotImplementedException();
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        throw new NotImplementedException();
    }

    protected override bool TryDequeue(Task task) => false;

    sealed class TaskQueueNode : IRunnable
    {
        readonly ExecutorTaskScheduler scheduler;
        readonly Task task;

        public TaskQueueNode(ExecutorTaskScheduler scheduler, Task task)
        {
            this.scheduler = scheduler;
            this.task = task;
        }

        public void Run() => scheduler.TryExecuteTask(task);
    }
}
