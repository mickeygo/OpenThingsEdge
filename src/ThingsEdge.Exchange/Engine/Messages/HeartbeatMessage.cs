using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Messages;

/// <summary>
/// 心跳消息。
/// </summary>
/// <param name="ChannelName">通道名称</param>
/// <param name="Device">设备信息</param>
/// <param name="Tag">标记</param>
/// <param name="Self">读取的标记值</param>
/// <param name="IsConnected">是否处于连接状态</param>
/// <param name="IsOnlySign">是否仅仅为信号</param>
internal sealed record HeartbeatMessage(
    string ChannelName,
    Device Device,
    Tag Tag,
    PayloadData Self,
    bool IsConnected,
    bool IsOnlySign = false) : IMessage;
