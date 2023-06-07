using Microsoft.Extensions.Hosting;

namespace ThingsEdge.Common.Queue;

/// <summary>
/// 任务队列的后台服务
/// </summary>
internal sealed class TaskQueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<TaskQueuedHostedService> _logger;

    public TaskQueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<TaskQueuedHostedService> logger) =>
        (_taskQueue, _logger) = (taskQueue, logger);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{TaskQueuedHostedService} is running. {NewLine}Tap W to add a work item to the background queue.",
            nameof(TaskQueuedHostedService), Environment.NewLine);

        return ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Func<CancellationToken, ValueTask>? workItem = await _taskQueue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TaskQueuedHostedService}] Error occurred executing task work item.", nameof(TaskQueuedHostedService));
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{TaskQueuedHostedService} is stopping.", nameof(TaskQueuedHostedService));
        await base.StopAsync(stoppingToken);
    }
}
