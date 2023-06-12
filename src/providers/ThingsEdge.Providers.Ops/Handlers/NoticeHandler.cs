using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Notice"/> 事件处理器。
/// </summary>
internal sealed class NoticeHandler : INotificationHandler<NoticeEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly IForwarderFactory _forwarderFactory;
    private readonly ILogger _logger;

    public NoticeHandler(IEventPublisher publisher, 
        ITagDataSnapshot tagDataSnapshot,
        IForwarderFactory forwarderFacory, 
        ILogger<NoticeHandler> logger)
    {
        _publisher = publisher;
        _tagDataSnapshot = tagDataSnapshot;
        _forwarderFactory = forwarderFacory;
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

        // 先提取上一次触发点的值
        var lastPayload = _tagDataSnapshot.Get(notification.Tag.TagId)?.Data;

        // 设置标记值快照。
        _tagDataSnapshot.Change(message.Values);

        // 发布标记数据请求事件（不用等待）。
        await _publisher.Publish(MessageRequestEvent.Create(message, lastPayload), PublishStrategy.ParallelNoWait, cancellationToken).ConfigureAwait(false);

        // 发送消息。
        var result = await _forwarderFactory.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            _logger.LogError("[Notice] 推送消息失败，设备: {DeviceName}, 标记: {Name}, 地址: {Address}, 错误: {ErrorMessage}",
                message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, result.ErrorMessage);
        }
    }
}
