using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Events;
using ThingsEdge.Exchange.Internal;

namespace ThingsEdge.Exchange.Engine.Monitors;

/// <summary>
/// 心跳监控
/// </summary>
internal sealed class HeartbeatMonitor : AbstractMonitor
{
    private readonly IProducer _producer;
    private readonly ExchangeConfig _opsConfig;
    private readonly ILogger _logger;

    public HeartbeatMonitor(IProducer producer,
        IOptions<ExchangeConfig> opsConfig,
        ILogger<HeartbeatMonitor> logger)
    {
        _producer = producer;
        _opsConfig = opsConfig.Value;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Heartbeat); // 所有标记为心跳的都进行监控。
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                var pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第二次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (!TagDataCache.CompareAndSwap(tag.TagId, false))
                            {
                                // 任务取消时，发布设备心跳断开事件。
                                await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag))).ConfigureAwait(false);
                            }

                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            if (!TagDataCache.CompareAndSwap(tag.TagId, false))
                            {
                                // 连接断开时，发布设备心跳断开事件。
                                await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag))).ConfigureAwait(false);
                            }

                            continue;
                        }

                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            _logger.LogError("[HeartbeatMonitor] Heartbeat 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 心跳标记数据类型必须为 bool 或 int16
                        if (CheckOn(data!))
                        {
                            if (GlobalSettings.HeartbeatShouldAckZero)
                            {
                                // 数据回写失败不影响，下一次轮询继续处理
                                await connector.WriteAsync(tag, SetOff2(tag)).ConfigureAwait(false);
                            }

                            if (!TagDataCache.CompareAndSwap(tag.TagId, true))
                            {
                                // 发布心跳正常事件。
                                await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, true, data!)).ConfigureAwait(false);
                            }
                        }

                        // 发布心跳信号事件（仅记录值）。
                        await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, true, data!, true)).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[HeartbeatMonitor] Heartbeat 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                               device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }
    }
}
