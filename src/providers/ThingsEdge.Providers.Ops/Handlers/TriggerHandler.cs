using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Trigger"/> 事件处理器。
/// </summary>
internal sealed class TriggerHandler : INotificationHandler<TriggerEvent>
{
    private readonly IEventPublisher _publisher;
    private readonly IForwarder _forwarder;
    private readonly ILogger _logger;

    public TriggerHandler(IEventPublisher publisher, IForwarder forwarder, ILogger<TriggerHandler> logger)
    {
        _publisher = publisher;
        _forwarder = forwarder;
        _logger = logger;
    }

    public async Task Handle(TriggerEvent notification, CancellationToken cancellationToken)
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
        var result = await _forwarder.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            // TODO: 推送失败状态

            _logger.LogError("推送消息失败，设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 错误: {Err}",
                message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, result.ErrorMessage);

            // 写入错误代码到设备
            if (notification.Connector.CanConnect)
            {
                var (ok4, err4) = await notification.Connector.WriteAsync(notification.Tag, result.Code).ConfigureAwait(false);
                if (!ok4)
                {
                    _logger.LogError("回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 错误: {Err}",
                        message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, err4);
                }
            }

            return;
        }

        // 连接器已断开。
        if (!notification.Connector.CanConnect)
        {
            return;
        }

        // 若存在子数据，则先回写。
        bool hasError = false;
        foreach (var (tagName, tagValue) in result.Data.CallbackItems)
        {
            // 通过 tagName 找到对应的 Tag 标记。
            // 注意：回写标记与触发标记处于同一级别，位于设备下或是分组中。
            Tag? tag2 = (tagGroup?.Tags ?? notification.Device.Tags).FirstOrDefault(s => s.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
            if (tag2 == null)
            {
                _logger.LogError("地址表中没有找到要回写的标记，设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 回写标记: {TagName}",
                    message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, tagName);

                hasError = true;
                break;
            }

            var (ok2, err2) = await notification.Connector.WriteAsync(tag2!, tagValue!).ConfigureAwait(false);
            if (!ok2)
            {
                _logger.LogError("回写标记数据失败, 设备: {DeviceName},标记: {TagName}, 地址: {TagAddress}, 错误: {Err}", 
                    message.Schema.DeviceName, tag2!.Name, tag2.Address, err2);

                hasError = true;
                break;
            }
        }

        // 回写标记状态。
        int tagCode = tagCode = hasError ? (int)ErrorCode.CallbackItemError : result.Data.State;
        var (ok3, err3) = await notification.Connector.WriteAsync(notification.Tag, tagCode).ConfigureAwait(false);
        if (!ok3)
        {
            _logger.LogError("回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}, 地址: {TagAddress}, 错误: {Err}",
                message.Schema.DeviceName, notification.Tag.Name, notification.Tag.Address, err3);
        }
    }
}
