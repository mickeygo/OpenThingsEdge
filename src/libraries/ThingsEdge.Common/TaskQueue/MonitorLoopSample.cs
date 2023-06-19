namespace ThingsEdge.Common.TaskQueue;

/// <summary>
/// 监控事件示例
/// </summary>
public sealed class MonitorLoopSample
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<MonitorLoopSample> _logger;
    private readonly CancellationToken _cancellationToken;

    public MonitorLoopSample(
        IBackgroundTaskQueue taskQueue,
        ILogger<MonitorLoopSample> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _cancellationToken = applicationLifetime.ApplicationStopping;
    }

    public void StartMonitorLoop()
    {
        _logger.LogInformation("{MonitorAsync} loop is starting.", nameof(MonitorAsync));

        // Run a console user input loop in a background thread
        Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            // Monitor Someting ...
            await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
        }
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        int delayLoop = 0;
        var guid = Guid.NewGuid();
        _logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++delayLoop;

            _logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }

        string format = delayLoop switch
        {
            3 => "Queued Background Task {Guid} is complete.",
            _ => "Queued Background Task {Guid} was cancelled."
        };

        _logger.LogInformation(format, guid);
    }
}
