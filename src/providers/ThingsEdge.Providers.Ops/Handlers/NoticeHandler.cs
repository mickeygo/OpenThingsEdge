using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Notice"/> 事件处理器。
/// </summary>
internal sealed class NoticeHandler : INotificationHandler<NoticeEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly IHttpForwarder _httpForwarder;
    private readonly ILogger _logger;

    public NoticeHandler(IEventPublisher publisher, IHttpForwarder httpForwarder, ILogger<NoticeHandler> logger)
    {
        _publisher = publisher;
        _httpForwarder = httpForwarder;
        _logger = logger;
    }

    public async Task Handle(NoticeEvent notification, CancellationToken cancellationToken)
    {
        var tagGroup = notification.Device.GetTagGroup(notification.Tag.TagId);
        var message = new RequestMessage
        {
            Schema = new()
            {
                ChannelName = "", // 可不用设置
                DeviceName = notification.Device.Name,
                TagGroupName = tagGroup?.Name,
            },
            Flag = notification.Tag.Flag,
        };
        message.Values.Add(notification.Self);

        // 发布标记数据读取消息。
        await _publisher.Publish(new TagValueReadEvent { Tag = notification.Tag, Value = notification.Self! }, 
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);

        // 读取触发标记下的子数据。
        foreach (var normalTag in notification.Tag.NormalTags)
        {
            // TODO: 思考如何将子数据地址合并，减少多次读取产生的性能开销。

            var (ok, data, err) = await notification.Connector.ReadAsync(normalTag).ConfigureAwait(false);
            if (!ok)
            {
                _logger.LogError("读取子标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                    message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, err);

                continue;
            }

            message.Values.Add(data!);

            // 发布标记数据读取消息。
            await _publisher.Publish(new TagValueReadEvent { Tag = normalTag, Value = data! },
                PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);
        }

        // 发送消息。
        var result = await _httpForwarder.SendAsync("/api/iotgateway/notice", message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            // TODO: 推送失败状态

            _logger.LogError("推送消息失败，设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 错误: {Err}",
                message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, result.ErrorMessage);
        }
    }
}
