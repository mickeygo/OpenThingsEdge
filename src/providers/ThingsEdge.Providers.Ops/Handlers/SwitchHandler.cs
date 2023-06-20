using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Handlers.Curve;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Switch"/> 事件处理器。
/// </summary>
internal sealed class SwitchHandler : INotificationHandler<SwitchEvent>
{
    private const int AllowMaxWriteCount = 3072; // 文件中允许写入最大的次数。

    private readonly IEventPublisher _publisher;
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly SwitchContainer _container;
    private readonly CurveStorage _curveStorage;
    private readonly OpsConfig _config;
    private readonly ILogger _logger;

    public SwitchHandler(IEventPublisher publisher,
        ITagDataSnapshot tagDataSnapshot,
        SwitchContainer container,
        CurveStorage curveStorage,
        IOptionsMonitor<OpsConfig> config,
        ILogger<SwitchHandler> logger)
    {
        _publisher = publisher;
        _tagDataSnapshot = tagDataSnapshot;
        _container = container;
        _curveStorage = curveStorage;
        _config = config.CurrentValue;
        _logger = logger;
    }

    public async Task Handle(SwitchEvent notification, CancellationToken cancellationToken)
    {
        // 开关信号
        if (notification.IsSwitchSignal)
        {
            var tagGroup = notification.Device.GetTagGroup(notification.Tag.TagId);
            var message = new RequestMessage
            {
                Schema = new()
                {
                    ChannelName = notification.ChannelName,
                    DeviceName = notification.Device.Name,
                    TagGroupName = tagGroup?.Name,
                },
                Flag = notification.Tag.Flag,
            };
            message.Values.Add(notification.Self);

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
                        sn = data1!.GetString();
                        message.Values.Add(data1!);
                    }
                    else
                    {
                        _logger.LogError("[Switch] 读取SwitchSN标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err1);
                    }
                }

                var noTag = notification.Tag.NormalTags.FirstOrDefault(s => s.Usage == TagUsage.SwitchNo);
                if (noTag != null)
                {
                    var (ok3, data3, err3) = await notification.Connector.ReadAsync(noTag).ConfigureAwait(false);
                    if (ok3)
                    {
                        no = data3!.GetString();
                        message.Values.Add(data3!);
                    }
                    else
                    {
                        _logger.LogError("[Switch] 读取SwitchNo标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err3);
                    }
                }

                // 获取或创建数据写入器（即使 sn 和 no 读取失败继续）
                var writer = _container.GetOrCreate(notification.Tag.TagId, _curveStorage.BuildCurveFilePath(sn, tagGroup?.Name, no));

                // 添加头信息
                var header = notification.Tag.NormalTags.Where(s => s.Usage == TagUsage.SwitchCurve).Select(s => s.Keynote);
                writer.WriteHeader(header);
            }
            else if (notification.State == SwitchState.Off)
            {
                // 可根据返回的文件路径做其他处理。
                if (_container.TryRemove(notification.Tag.TagId, out var filepath))
                {
                    var (ok, err) = await _curveStorage.TryCopyAsync(filepath, cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        _logger.LogError("[Switch] 拷贝曲线失败，文件：{Filepath}, 错误：{Err}", Path.GetFileName(filepath), err);
                    }
                }
            }

            // 先提取上一次触发点的值
            var lastPayload = _tagDataSnapshot.Get(notification.Tag.TagId)?.Data;

            // 设置标记值快照。
            _tagDataSnapshot.Change(message.Values);

            // 发布标记数据请求事件。
            await _publisher.Publish(DirectMessageRequestEvent.Create(message, lastPayload),
                PublishStrategy.ParallelNoWait, cancellationToken).ConfigureAwait(false);

            return;
        }

        // 开关数据
        if (!_container.TryGet(notification.Tag.TagId, out var writer2))
        {
            return;
        }

        // 检测是否已到达写入行数的上限，用于防止写入数据过程导致数据过大。
        if (writer2.WrittenCount > AllowMaxWriteCount)
        {
            _logger.LogError("[Switch] 文件写入数据已达到设置上限, 设备: {Name}, 标记: {Name}，地址: {Address}",
                notification.Device.Name, notification.Tag.Name, notification.Tag.Address);

            return;
        }

        // 读取触发标记下的子数据。
        var (ok2, normalPaydatas, err2) = await notification.Connector.ReadMultiAsync(
                notification.Tag.NormalTags.Where(s => s.Usage == TagUsage.SwitchCurve), _config.AllowReadMulti).ConfigureAwait(false);
        if (!ok2)
        {
            _logger.LogError("[Switch] 批量读取子标记值失败, 设备: {Name}, 错误: {Err}", notification.Device.Name, err2);
            return;
        }

        // 设置标记值快照。
        _tagDataSnapshot.Change(normalPaydatas!);

        // 检测写入对象是否已关闭
        if (!writer2.IsClosed)
        {
            try
            {
                writer2.WriteLineBody(normalPaydatas!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Switch] 曲线数据写入文件失败, 设备: {Name}, 标记: {Name}，地址: {Address}",
                    notification.Device.Name, notification.Tag.Name, notification.Tag.Address);
            }
        }
    }
}
