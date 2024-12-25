using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Exchange.Engine.Handlers;

/// <summary>
/// 心跳消息处理器。
/// </summary>
internal sealed class HeartbeatMessageHandler(IHeartbeatForwarderProxy forwarderProxy, ITagDataSnapshot tagDataSnapshot) : IHeartbeatMessageHandler
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

        await forwarderProxy.ChangeAsync(new(message.ChannelName, message.Device, message.Tag, message.IsConnected), cancellationToken).ConfigureAwait(false);
    }
}
