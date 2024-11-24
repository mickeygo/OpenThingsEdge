using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Exchange.Engine.Handler;

internal sealed class HeartbeatMessageHandler(INativeHeartbeatForwarder forwarder, ITagDataSnapshot tagDataSnapshot) : IMessageHandler<HeartbeatMessage>
{
    public async Task HandleAsync(HeartbeatMessage message, CancellationToken cancellationToken)
    {
        // 设置标记值快照。
        tagDataSnapshot.Change(message.Self);

        // 若是信号值，则中断处理。
        if (message.IsOnlySign)
        {
            return;
        }

        HeartbeatForwarderContext context = new()
        {
            ChannelName = message.ChannelName,
            Device = message.Device,
            Tag = message.Tag,
            IsOnline = message.IsConnected,
        };

        await forwarder.ChangeAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
