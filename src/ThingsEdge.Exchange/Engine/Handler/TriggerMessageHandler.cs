using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Exchange.Engine.Handler;

/// <summary>
/// 触发消息处理器。
/// </summary>
internal sealed class TriggerMessageHandler(ITagDataSnapshot tagDataSnapshot,
    IRequestForwarderProvider requestForwarderProvider,
    IOptions<ExchangeOptions> options,
    ILogger<TriggerMessageHandler> logger) : ITriggerMessageHandler
{
    public async Task HandleAsync(TriggerMessage message, CancellationToken cancellationToken)
    {
        var tagGroup = message.Device.GetTagGroup(message.Tag.TagId);
        var reqMessage = new RequestMessage
        {
            Schema = new()
            {
                ChannelName = message.ChannelName,
                DeviceName = message.Device.Name,
                TagGroupName = tagGroup?.Name,
            },
            Flag = message.Tag.Flag,
        };
        reqMessage.Values.Add(message.Self);

        // 读取触发标记下的子数据。
        var (ok, normalPaydatas, err) = await message.Connector.ReadMultiAsync(message.Tag.NormalTags, options.Value.AllowReadMulti).ConfigureAwait(false);
        if (!ok)
        {
            logger.LogError("[TriggerMessageHandler] 批量读取子标记值异常, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                message.Device.Name, message.Tag.Name, message.Tag.Address, err);

            // 写入错误代码到设备
            if (message.Connector.CanConnect)
            {
                var (ok5, _, err5) = await message.Connector.WriteAsync(message.Tag, (int)ExchangeErrorCode.MultiReadItemError).ConfigureAwait(false);
                if (!ok5)
                {
                    logger.LogError("[TriggerMessageHandler] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        message.Device.Name, message.Tag.Name, message.Tag.Address, err5);

                    AckTagSet(message.Tag.TagId);
                }
            }
        }
        else
        {
            reqMessage.Values.AddRange(normalPaydatas!);
        }

        // 设置标记值快照。
        tagDataSnapshot.Change(reqMessage.Values);

        // 读取数据出错，直接退出
        if (!ok)
        {
            return;
        }

        // 获取注册的发送请求对象，没有注册则后续不处理。
        var requestForwarder = requestForwarderProvider.GetForwarder();
        if (requestForwarder == null)
        {
            return;
        }

        // 发送消息。
        var result = await requestForwarder.SendAsync(reqMessage, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess())
        {
            logger.LogError("[TriggerMessageHandler] 推送消息失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                message.Device.Name, message.Tag.Name, message.Tag.Address, result.ErrorMessage);

            // 将错误代码写入到设备
            if (message.Connector.CanConnect)
            {
                var (ok4, _, err4) = await message.Connector.WriteAsync(message.Tag, ChangeWhenEqTriggerCondValue(result.Code)).ConfigureAwait(false);
                if (!ok4)
                {
                    logger.LogError("[TriggerMessageHandler] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        message.Device.Name, message.Tag.Name, message.Tag.Address, err4);
                    AckTagSet(message.Tag.TagId);
                }
            }

            return;
        }

        // 连接器已断开。
        if (!message.Connector.CanConnect)
        {
            return;
        }

        // 若存在回调数据，则先回写。
        var hasError = false;
        if (result.Data?.CallbackItems != null)
        {
            foreach (var (tagName, tagValue) in result.Data.CallbackItems)
            {
                // 通过 tagName 找到对应的 Tag 标记。
                // 注意：回写标记与触发标记处于同一级别，位于设备下或是分组中。
                var tag2 = (tagGroup?.CallbackTags ?? message.Device.CallbackTags)
                    .FirstOrDefault(s => s.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
                if (tag2 == null)
                {
                    logger.LogError("[TriggerMessageHandler] 地址表中没有找到要回写的标记, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}",
                        message.Device.Name, message.Tag.Name, message.Tag.Address);

                    hasError = true;
                    break;
                }

                var (ok2, formatedData2, err2) = await message.Connector.WriteAsync(tag2!, tagValue!).ConfigureAwait(false);
                if (!ok2)
                {
                    logger.LogError("[TriggerMessageHandler] 回写标记数据失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                        message.Device.Name, message.Tag.Name, message.Tag.Address, err2);

                    hasError = true;
                    break;
                }

                // 设置回写的标记值快照。
                tagDataSnapshot.Change(tag2, formatedData2!);
            }
        }

        // 将返回的标记状态写入到设备
        var tagCode = hasError ? (int)ExchangeErrorCode.CallbackItemError : result.Data!.State;
        var (ok3, formatedData3, err3) = await message.Connector.WriteAsync(message.Tag, ChangeWhenEqTriggerCondValue(tagCode)).ConfigureAwait(false);
        if (!ok3)
        {
            logger.LogError("[TriggerMessageHandler] 回写触发标记状态失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                message.Device.Name, message.Tag.Name, message.Tag.Address, err3);

            AckTagSet(message.Tag.TagId);
        }

        // 设置回写的标记状态快照。
        tagDataSnapshot.Change(message.Tag, formatedData3!);
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
        if (options.Value.AckRetryMaxCount > 0)
        {
            TagDataCache.Ack(tagId, options.Value.AckRetryMaxCount);
        }
    }

    /// <summary>
    /// 在返回值与触发值相等时，将返回的状态值更改为 0。
    /// </summary>
    /// <param name="tagCode"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ChangeWhenEqTriggerCondValue(int tagCode)
    {
        return tagCode == options.Value.TagTriggerConditionValue ? 0 : tagCode;
    }
}
