using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Exchange.Engine.Handlers;

/// <summary>
/// 通知消息处理器。
/// </summary>
internal sealed class NoticeMessageHandler(
    INoticeForwarder forwarderProxy,
    ITagDataSnapshot tagDataSnapshot,
    IOptions<ExchangeOptions> options,
    ILogger<NoticeMessageHandler> logger) : INoticeMessageHandler
{
    public async Task HandleAsync(NoticeMessage message, CancellationToken cancellationToken)
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
        };
        reqMessage.Values.Add(message.Self);

        if (message.Tag.NormalTags.Count > 0)
        {
            // 读取触发标记下的子数据。
            var (ok, normalPayloads, err) = await message.Connector.ReadMultiAsync(message.Tag.NormalTags, options.Value.AllowReadMultiple).ConfigureAwait(false);
            if (!ok)
            {
                logger.LogError("[NoticeMessageHandler] 批量读取子标记值异常, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                    message.Device.Name, message.Tag.Name, message.Tag.Address, err);
            }
            else
            {
                reqMessage.Values.AddRange(normalPayloads!);
            }
        }

        // 设置标记值快照。
        tagDataSnapshot.Change(reqMessage.Values);

        // 发送消息
        await forwarderProxy.ReceiveAsync(new NoticeContext(reqMessage), cancellationToken).ConfigureAwait(false);
    }
}
