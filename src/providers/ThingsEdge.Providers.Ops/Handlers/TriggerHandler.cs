using ThingsEdge.Common.EventBus;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Trigger"/> 事件处理器。
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
        await _publisher.Publish(TagValueChangedEvent.Create(notification.Self!),
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);

        // 读取触发标记下的子数据。
        var (ok, normalPaydatas, err) = await notification.Connector.ReadMultiAsync(notification.Tag.NormalTags).ConfigureAwait(false);
        if (!ok)
        {
            string msg1 = $"批量读取子标记值异常, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {err}";
            _logger.LogError(msg1);
            await _publisher.Publish(MessageLoggedEvent.Error(msg1), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

            // 写入错误代码到设备
            if (notification.Connector.CanConnect)
            {
                var (ok5, err5) = await notification.Connector.WriteAsync(notification.Tag, (int)ErrorCode.MultiReadItemError).ConfigureAwait(false);
                if (!ok5)
                {
                    string msg2 = $"回写触发标记状态失败, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {err5}";
                    _logger.LogError(msg2);
                    await _publisher.Publish(MessageLoggedEvent.Error(msg2), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                }
            }

            return;
        }

        message.Values.AddRange(normalPaydatas!);

        // 发布标记数据读取消息。
        await _publisher.Publish(TagValueChangedEvent.Create(normalPaydatas!),
            PublishStrategy.AsyncContinueOnException, cancellationToken).ConfigureAwait(false);

        // 发送消息。
        var result = await _forwarder.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            string msg1 = $"推送消息失败, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {result.ErrorMessage}";
            _logger.LogError(msg1);
            await _publisher.Publish(MessageLoggedEvent.Error(msg1), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

            // 写入错误代码到设备
            if (notification.Connector.CanConnect)
            {
                var (ok4, err4) = await notification.Connector.WriteAsync(notification.Tag, result.Code).ConfigureAwait(false);
                if (!ok4)
                {
                    string msg2 = $"回写触发标记状态失败, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {err4}";
                    _logger.LogError(msg2);
                    await _publisher.Publish(MessageLoggedEvent.Error(msg2), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
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
                string msg = $"地址表中没有找到要回写的标记, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}";
                _logger.LogError(msg);
                await _publisher.Publish(MessageLoggedEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                hasError = true;
                break;
            }

            var (ok2, err2) = await notification.Connector.WriteAsync(tag2!, tagValue!).ConfigureAwait(false);
            if (!ok2)
            {
                string msg = $"回写标记数据失败, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {err2}";
                _logger.LogError(msg);
                await _publisher.Publish(MessageLoggedEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                hasError = true;
                break;
            }
        }

        // 回写标记状态。
        int tagCode = tagCode = hasError ? (int)ErrorCode.CallbackItemError : result.Data.State;
        var (ok3, err3) = await notification.Connector.WriteAsync(notification.Tag, tagCode).ConfigureAwait(false);
        if (!ok3)
        {
            string msg = $"回写触发标记状态失败, 设备: {message.Schema.DeviceName}, 标记: {notification.Tag.Name}, 地址: {notification.Tag.Address}, 错误: {err3}";
            _logger.LogError(msg);
            await _publisher.Publish(MessageLoggedEvent.Error(msg), PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
        }
    }
}
