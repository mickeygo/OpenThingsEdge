using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;

namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// 触发监控
/// </summary>
internal sealed class TriggerMonitor : AbstractMonitor, ITransientDependency
{
    private readonly IProducer _producer;
    private readonly OpsConfig _config;
    private readonly ILogger _logger;

    public TriggerMonitor(IProducer producer,
        IOptionsMonitor<OpsConfig> config,
        ILogger<TriggerMonitor> logger)
    {
        _producer = producer;
        _config = config.CurrentValue;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _config.DefaultScanRate;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第一次检测
                        if (cancellationToken .IsCancellationRequested)
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
                            _logger.LogError("[TriggerMonitor] Trigger 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{Address}，错误：{err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 校验触发标记
                        var state = data!.GetInt(); // 触发标记还可能包含状态码信息。

                        // 必须先检测并更新标记状态值（开启回执校验），若值有变动且触发标记值为 1 则推送数据。
                        if (!TagValueSet.CompareAndSwap(tag.TagId, state, true) && state == 1)
                        {
                            // 发布触发事件
                            await _producer.ProduceAsync(new TriggerEvent
                            {
                                Connector = connector,
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                Self = data,
                            }).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TriggerMonitor] Trigger 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{Address}",
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }
    }
}
