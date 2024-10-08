﻿using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Providers.Ops.Internal;
using ThingsEdge.Providers.Ops.Snapshot;
using ThingsEdge.Router.Forwarders;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="TagFlag.Trigger"/> 事件处理器。
/// </summary>
internal sealed class TriggerHandler : INotificationHandler<TriggerEvent>
{
    private readonly ITagDataSnapshot _tagDataSnapshot;
    private readonly IRequestForwarderProvider _requestForwarderProvider;
    private readonly OpsConfig _config;
    private readonly ILogger _logger;

    public TriggerHandler(ITagDataSnapshot tagDataSnapshot,
        IRequestForwarderProvider requestForwarderProvider,
        IOptions<OpsConfig> config,
        ILogger<TriggerHandler> logger)
    {
        _tagDataSnapshot = tagDataSnapshot;
        _requestForwarderProvider = requestForwarderProvider;
        _config = config.Value;
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

        // 读取触发标记下的子数据。
        var (ok, normalPaydatas, err) = await notification.Connector.ReadMultiAsync(notification.Tag.NormalTags, _config.AllowReadMulti).ConfigureAwait(false);
        if (!ok)
        {
            _logger.LogError("[Trigger] 批量读取子标记值异常, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err);

            // 写入错误代码到设备
            if (notification.Connector.CanConnect)
            {
                var (ok5, _, err5) = await notification.Connector.WriteAsync(notification.Tag, (int)ErrCode.MultiReadItemError).ConfigureAwait(false);
                if (!ok5)
                {
                    _logger.LogError("[Trigger] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err5);

                    AckTagSet(notification.Tag.TagId);
                }
            }
        }
        else
        {
            message.Values.AddRange(normalPaydatas!);
        }

        // 设置标记值快照。
        _tagDataSnapshot.Change(message.Values);

        // 读取数据出错，直接退出
        if (!ok)
        {
            return;
        }

        // 获取注册的发送请求对象，没有注册则后续不处理。
        var requestForwarder = _requestForwarderProvider.GetForwarder();
        if (requestForwarder == null) 
        { 
            return;
        }

        // 发送消息。
        var result = await requestForwarder.SendAsync(message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            _logger.LogError("[Trigger] 推送消息失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                notification.Device.Name, notification.Tag.Name, notification.Tag.Address, result.ErrorMessage);

            // 写入错误代码到设备
            if (notification.Connector.CanConnect)
            {
                var (ok4, _, err4) = await notification.Connector.WriteAsync(notification.Tag, ChangeWhenEqTriggerCondValue(result.Code)).ConfigureAwait(false);
                if (!ok4)
                {
                    _logger.LogError("[Trigger] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err4);

                    AckTagSet(notification.Tag.TagId);
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
        if (result.Data?.CallbackItems != null)
        {
            foreach (var (tagName, tagValue) in result.Data.CallbackItems)
            {
                // 通过 tagName 找到对应的 Tag 标记。
                // 注意：回写标记与触发标记处于同一级别，位于设备下或是分组中。
                Tag? tag2 = (tagGroup?.CallbackTags ?? notification.Device.CallbackTags)
                    .FirstOrDefault(s => s.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
                if (tag2 == null)
                {
                    _logger.LogError("[Trigger] 地址表中没有找到要回写的标记, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}",
                        notification.Device.Name, notification.Tag.Name, notification.Tag.Address);

                    hasError = true;
                    break;
                }

                var (ok2, formatedData2, err2) = await notification.Connector.WriteAsync(tag2!, tagValue!).ConfigureAwait(false);
                if (!ok2)
                {
                    _logger.LogError("[Trigger] 回写标记数据失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err2);

                    hasError = true;
                    break;
                }

                // 设置回写的标记值快照。
                _tagDataSnapshot.Change(tag2, formatedData2!);
            }
        }

        // 回写标记状态。
        int tagCode = hasError ? (int)ErrCode.CallbackItemError : result.Data!.State;
        var (ok3, formatedData3, err3) = await notification.Connector.WriteAsync(notification.Tag, ChangeWhenEqTriggerCondValue(tagCode)).ConfigureAwait(false);
        if (!ok3)
        {
            _logger.LogError("[Trigger] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err3);

            AckTagSet(notification.Tag.TagId);
        }

        // 设置回写的标记状态快照。
        _tagDataSnapshot.Change(notification.Tag, formatedData3!);
    }

    /// <summary>
    /// 若标志值回写失败，标志位值不会发生跳变（PLC值和TagSet值都为1），这样导致该标志位后续的处理逻辑直接被跳过。
    /// </summary>
    /// <remarks>
    /// 这里处理方式是：在回写失败时，设置回执（Trigger 监控器中必须启动回执比较）。注：接收数据后的逻辑需要做幂等处理（对已处理的数据直接返回 OK 状态）。
    /// </remarks>
    /// <param name="tagId"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AckTagSet(string tagId)
    {
        if (_config.AckWhenCallbackError)
        {
            TagValueSet.Ack(tagId, _config.AckMaxVersion);
        }
    }

    /// <summary>
    /// 在返回值与触发值相等时，将返回的状态值更改为 0。
    /// </summary>
    /// <param name="tagCode"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ChangeWhenEqTriggerCondValue(int tagCode)
    {
        return tagCode == GlobalSettings.TagTriggerConditionValue ? 0 : tagCode;
    }
}
