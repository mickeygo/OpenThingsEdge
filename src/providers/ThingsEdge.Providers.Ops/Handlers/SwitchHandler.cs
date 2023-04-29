using ThingsEdge.Common.EventBus;
using ThingsEdge.Contracts.Devices;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Switch"/> 事件处理器。
/// </summary>
internal sealed class SwitchHandler : INotificationHandler<SwitchEvent>
{
    private const int AllowMaxWriteCount = 3072; // 文件中允许写入最大的次数。

    private readonly IEventPublisher _publisher;
    private readonly SwitchContainer _container;
    private readonly CurveStorage _curveStorage;
    private readonly ILogger _logger;

    public SwitchHandler(IEventPublisher publisher, SwitchContainer container, CurveStorage curveStorage, ILogger<SwitchHandler> logger)
    {
        _publisher = publisher;
        _container = container;
        _curveStorage = curveStorage;
        _logger = logger;
    }

    public async Task Handle(SwitchEvent notification, CancellationToken cancellationToken)
    {
        // 开关信号
        if (notification.IsSwitchSignal)
        {
            List<PayloadData> payloads = new()
            {
                notification.Self!,
            };

            if (notification.State == SwitchState.On)
            {
                string? sn = null, no = null;
                // 码和序号
                var snTag = notification.Tag.NormalTags.FirstOrDefault(s => s.Usage == TagUsage.SwitchSN);
                if (snTag != null)
                {
                    var (ok1, data1, err1) = await notification.Connector.ReadAsync(snTag).ConfigureAwait(false);
                    if (ok1)
                    {
                        sn = data1.GetString();
                        payloads.Add(data1);
                    }
                    else
                    {
                        _logger.LogError("读取SwitchSN标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err1);
                    }
                }

                var noTag = notification.Tag.NormalTags.FirstOrDefault(s => s.Usage == TagUsage.SwitchNo);
                if (noTag != null)
                {
                    var (ok3, data3, err3) = await notification.Connector.ReadAsync(noTag).ConfigureAwait(false);
                    if (ok3)
                    {
                        no = data3.GetString();
                        payloads.Add(data3);
                    }
                    else
                    {
                        _logger.LogError("读取SwitchNo标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err3);
                    }
                }

                var tagGroup = notification.Device.GetTagGroup(notification.Tag.TagId);
                var writer = _container.GetOrCreate(notification.Tag.TagId, _curveStorage.BuildCurveFilePath(tagGroup?.Name, sn, no)); 

                // 添加头信息
                var headers = string.Join(",", notification.Tag.NormalTags.Where(s => s.Usage == TagUsage.SwitchCurve).Select(s => s.Keynote));
                await writer.WriteLineAsync(headers).ConfigureAwait(false);
            }
            else if (notification.State == SwitchState.Off)
            {
                // 可根据返回的文件路径做其他处理。
                if (_container.TryRemove(notification.Tag.TagId, out var filepath))
                {
                    var (ok, err) = await _curveStorage.TryCopyAsync(filepath, cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        _logger.LogError("拷贝曲线失败，错误：{Err}", err);
                    }
                }
            }

            // 发布标记数据读取事件。
            await _publisher.Publish(TagValueChangedEvent.Create(payloads),
                PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);

            return;
        }

        // 开关数据
        if(!_container.TryGet(notification.Tag.TagId, out var writer2))
        {
            return;
        }

        // 检测是否已到达写入行数的上限，用于防止
        if (writer2.WrittenCount > AllowMaxWriteCount)
        {
            _logger.LogWarning("文件写入数据已达到设置上限, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}",
                    notification.Device.Name, notification.Tag.Name, notification.Tag.Address);

            return;
        }

        // 读取触发标记下的子数据。
        var (ok2, normalPaydatas, err2) = await notification.Connector.ReadMultiAsync(
            notification.Tag.NormalTags.Where(s => s.Usage == TagUsage.SwitchCurve)).ConfigureAwait(false);
        if (!ok2)
        {
            _logger.LogError("批量读取子标记值失败, 设备: {DeviceName}, 错误: {Err}", notification.Device.Name, err2);
            return;
        }

        // 检测写入对象是否已关闭
        if (!writer2.IsClosed)
        {
            try
            {
                await writer2.WriteLineAsync(string.Join(",", normalPaydatas!.Select(s => s.GetString()))).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "曲线数据写入文件失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}",
                    notification.Device.Name, notification.Tag.Name, notification.Tag.Address);
            }
        }
    }
}
