using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 监控通知的工作者。
/// </summary>
internal sealed class NoticeWorker(IMessageBroker<NoticeMessage> broker,
    IOptions<ExchangeOptions> options,
    ILogger<NoticeWorker> logger) : IWorker
{
    public Task RunAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tags = device.GetAllTags(TagFlag.Notice);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                var pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : options.Value.DefaultScanRate;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第一次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            logger.LogError("[NoticeWorker] Notice 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // EveryScan 模式下时每次都会发送， OnlyDataChanged 模式下在仅数据有跳变时才会发送。
                        if (tag.PublishMode is PublishMode.EveryScan
                            || (tag.PublishMode is PublishMode.OnlyDataChanged && TagHoldDataCache.CompareExchange(tag.TagId, data!.Value)))
                        {
                            // 发布通知事件
                            await broker.PushAsync(new NoticeMessage(connector, channelName, device, tag, data!), cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[NoticeWorker] Notice 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                                device.Name, tag.Name, tag.Address);
                    }
                }
            }, default);
        }

        return Task.CompletedTask;
    }
}
