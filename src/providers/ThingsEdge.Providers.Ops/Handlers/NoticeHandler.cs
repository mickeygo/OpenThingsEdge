using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Notice"/> 事件处理器。
/// </summary>
internal sealed class NoticeHandler : INotificationHandler<NoticeEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly OpsConfig _config;
    private readonly ILogger _logger;

    public NoticeHandler(IEventPublisher publisher, 
        ITagDataSnapshot tagDataSnapshot,
        IOptions<OpsConfig> config,
        ILogger<NoticeHandler> logger)
    {
        _publisher = publisher;
        _tagDataSnapshot = tagDataSnapshot;
        _config = config.Value;
        _logger = logger;
    }

    public async Task Handle(NoticeEvent notification, CancellationToken cancellationToken)
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

        if (notification.Tag.NormalTags.Count > 0)
        {
            // 读取触发标记下的子数据。
            var (ok, normalPaydatas, err) = await notification.Connector.ReadMultiAsync(notification.Tag.NormalTags, _config.AllowReadMulti).ConfigureAwait(false);
            if (!ok)
            {
                _logger.LogError("[Notice] 批量读取子标记值异常, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                    notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err);
            }
            else
            {
                message.Values.AddRange(normalPaydatas!);
            }
        }

        // 先提取上一次触发点的值
        var lastPayload = _tagDataSnapshot.Get(notification.Tag.TagId)?.Data;

        // 设置标记值快照。
        _tagDataSnapshot.Change(message.Values);

        // 发布标记数据请求事件（不用等待）。
        await _publisher.Publish(new NoticePostedEvent(message, lastPayload), PublishStrategy.ParallelNoWait, cancellationToken).ConfigureAwait(false);
    }
}
