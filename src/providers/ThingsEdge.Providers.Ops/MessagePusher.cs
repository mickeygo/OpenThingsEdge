using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

/// <summary>
/// 消息推送器。
/// </summary>
public sealed class MessagePusher : IMessagePusher
{
    private readonly ILogger _logger;

    public MessagePusher(ILogger<MessagePusher> logger)
    {
        _logger = logger;
    }

    public async Task PushAsync(DriverConnector connector, Tag tag, PayloadData? self)
    {
        // 构建请求消息
        var message = new RequestMessage
        {
            Schema = new()
            {
                ChannelName = "",
                DeviceName = "",
                TagGroupName = "",
            },
        };
        if (self != null)
        {
            message.Values.Add(self);
        }

        foreach (var normalTag in tag.NormalTags)
        {
            var (ok, data, err) = await connector.ReadAsync(normalTag);
            if (!ok)
            {
                _logger.LogError($"设备：{message.Schema.DeviceName}，标记：{tag.Name}，地址：{tag.Address}, 错误：{err}");
                continue;
            }

            message.Values.Add(data!);
        }

        // 推送消息
    }
}
