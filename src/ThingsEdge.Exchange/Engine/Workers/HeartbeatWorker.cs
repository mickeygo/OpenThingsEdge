using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Workers;

/// <summary>
/// 监控心跳的工作者。
/// </summary>
internal sealed class HeartbeatWorker(IMessageBroker<HeartbeatMessage> broker,
    IOptions<ExchangeOptions> options,
    ILogger<HeartbeatWorker> logger) : IWorker
{
    public Task RunAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tags = device.GetAllTags(TagFlag.Heartbeat); // 所有标记为心跳的都进行监控
        foreach (var tag in tags)
        {
            var pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : options.Value.DefaultScanRate;
            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第二次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (TagDataAccesstor.CompareAndExchange(tag.TagId, false))
                            {
                                // 任务取消时，发布设备心跳断开事件。
                                await broker.PushAsync(
                                    new HeartbeatMessage(channelName!, device, tag, WorkerUtils.CreateHeartbeatPayloadOff(tag, options.Value.HeartbeatListenUseHighLevel), false),
                                    cancellationToken).ConfigureAwait(false);
                            }

                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            if (TagDataAccesstor.CompareAndExchange(tag.TagId, false))
                            {
                                // 连接断开时，发布设备心跳断开事件。
                                await broker.PushAsync(
                                    new HeartbeatMessage(channelName!, device, tag, WorkerUtils.CreateHeartbeatPayloadOff(tag, options.Value.HeartbeatListenUseHighLevel), false),
                                    cancellationToken).ConfigureAwait(false);
                            }

                            continue;
                        }

                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            logger.LogError("[HeartbeatWorker] Heartbeat 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 心跳标记数据类型必须为 bool 或 int16
                        if (WorkerUtils.CheckHeartbeatOn(data!, options.Value.HeartbeatListenUseHighLevel))
                        {
                            if (options.Value.HeartbeatShouldAckZero)
                            {
                                // 数据回写失败不影响，下一次轮询继续处理
                                await connector.WriteAsync(tag, WorkerUtils.SetHeartbeatOff(tag, options.Value.HeartbeatListenUseHighLevel)).ConfigureAwait(false);
                            }

                            if (TagDataAccesstor.CompareAndExchange(tag.TagId, true))
                            {
                                // 发布心跳正常事件。
                                await broker.PushAsync(new HeartbeatMessage(channelName!, device, tag, data!, true), cancellationToken).ConfigureAwait(false);
                            }
                        }

                        // 发布心跳信号事件（仅记录值）。
                        await broker.PushAsync(new HeartbeatMessage(channelName!, device, tag, data!, true, true), cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[HeartbeatWorker] Heartbeat 接收数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                              device.Name, tag.Name, tag.Address);
                    }
                }
            }, default);
        }

        return Task.CompletedTask;
    }
}
