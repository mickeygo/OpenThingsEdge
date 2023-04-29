using ThingsEdge.Common.EventBus;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Notice"/> 事件处理器。
/// </summary>
internal sealed class NoticeHandler : INotificationHandler<NoticeEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly IForwarder _forwarder;
    private readonly ILogger _logger;

    public NoticeHandler(IEventPublisher publisher, IForwarder forwarder, ILogger<NoticeHandler> logger)
    {
        _publisher = publisher;
        _forwarder = forwarder;
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

        // 发布标记数据读取消息。
        // 注：Notice 只包含数据本身，不包含标记下的子数据。
        await _publisher.Publish(TagValueChangedEvent.Create(notification.Self),
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);

        // 发送消息。
        var result = await _forwarder.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            // TODO: 推送失败状态

            _logger.LogError("推送消息失败，设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 错误: {Err}",
                message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, result.ErrorMessage);
        }
    }
}
