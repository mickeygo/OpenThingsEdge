using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Events;

namespace ThingsEdge.Exchange.Engine.Monitors;

/// <summary>
/// 通知监控
/// </summary>
internal sealed class NoticeMonitor : AbstractMonitor
{
    private readonly IProducer _producer;
    private readonly ExchangeConfig _opsConfig;
    private readonly ILogger _logger;

    public NoticeMonitor(IProducer producer,
        IOptions<ExchangeConfig> opsConfig,
        ILogger<NoticeMonitor> logger)
    {
        _producer = producer;
        _opsConfig = opsConfig.Value;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Notice);
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
                            _logger.LogError("[NoticeMonitor] Notice 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 在仅数据变更才会发送模式下，会校验数据是否有跳变。
                        if (tag.PublishMode == PublishMode.OnlyDataChanged && TagDataCache.CompareAndSwap(tag.TagId, data!.Value))
                        {
                            continue;
                        }

                        // 发布通知事件
                        await _producer.ProduceAsync(new NoticeEvent
                        {
                            Connector = connector,
                            ChannelName = channelName,
                            Device = device,
                            Tag = tag,
                            Self = data,
                        }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[NoticeMonitor] Notice 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                                device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }
    }
}
