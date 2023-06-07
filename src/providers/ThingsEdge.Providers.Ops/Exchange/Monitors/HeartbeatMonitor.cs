using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// 心跳监控
/// </summary>
internal sealed class HeartbeatMonitor : AbstractMonitor, ITransientDependency
{
    private readonly IProducer _producer;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public HeartbeatMonitor(IProducer producer,
        IOptionsMonitor<OpsConfig> opsConfig, 
        ILogger<HeartbeatMonitor> logger)
    {
        _producer = producer;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Heartbeat); // 所有标记为心跳的都进行监控。
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第一次检测
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (!TagValueSet.CompareAndSwap(tag.TagId, false))
                            {
                                // 任务取消时，发布设备心跳断开事件。
                                await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag))).ConfigureAwait(false);
                            }

                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            if (!TagValueSet.CompareAndSwap(tag.TagId, false))
                            {
                                // 连接断开时，发布设备心跳断开事件。
                                await _producer.ProduceAsync(HeartbeatEvent.Create(channelName!, device, tag, false, SetOff(tag))).ConfigureAwait(false);
                            }

                            continue;
                        }

                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            string msg = $"[HeartbeatMonitor] Heartbeat 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}";
                            _logger.LogError(msg);
                            await _producer.ProduceAsync(LoggingMessageEvent.Error(msg)).ConfigureAwait(false);

                            continue;
                        }

                        // 心跳标记数据类型必须为 bool 或 int16
                        if (CheckOn(data!))
                        {
                            await connector.WriteAsync(tag, SetOff2(tag)).ConfigureAwait(false);

                            if (!TagValueSet.CompareAndSwap(tag.TagId, true))
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
                        string msg = $"[HeartbeatMonitor] Heartbeat 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _producer.ProduceAsync(LoggingMessageEvent.Error(msg)).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}
