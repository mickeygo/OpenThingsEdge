using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 监控触发的工作者。
/// </summary>
internal sealed class TriggerWorker(IMessageBroker<TriggerMessage> broker,
    IOptions<ExchangeOptions> options,
    ILogger<NoticeWorker> logger) : IWorker
{
    public Task RunAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tags = device.GetAllTags(TagFlag.Trigger);
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
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false); // short 类型
                        if (!ok)
                        {
                            logger.LogError("[TriggerWorker] Trigger 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{Address}，错误：{err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 校验触发标记（必须为 byte 或 short 类型）。
                        var state = data!.DataType switch
                        {
                            TagDataType.Byte => data.GetByte(),
                            TagDataType.Int => data.GetInt(),
                            _ => throw new InvalidOperationException(),
                        };

                        // 必须先检测并更新标记状态值（开启回执校验），若值有变动且达到触发标记条件时则推送数据。
                        if (!TagDataCache.CompareAndSwap(tag.TagId, state, true) && state == options.Value.TagTriggerConditionValue)
                        {
                            // 发布触发事件
                            await broker.PushAsync(new TriggerMessage(connector, channelName, device, tag, data!), cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[TriggerWorker] Trigger 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{Address}",
                            device.Name, tag.Name, tag.Address);
                    }
                }
            }, default);
        }

        return Task.CompletedTask;
    }
}
