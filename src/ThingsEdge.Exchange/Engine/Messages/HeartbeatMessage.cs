using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Messages;

/// <summary>
/// 心跳消息。
/// </summary>
/// <param name="ChannelName">通道名称</param>
/// <param name="Device">设备信息</param>
/// <param name="Tag">信号标记</param>
/// <param name="Self">读取的标记值</param>
/// <param name="IsConnected">是否处于连接状态</param>
/// <param name="IsOnlySign">是否仅仅为信号，信号表示值不受跳变影响，每次轮询时都会触发消息发送</param>
internal sealed record HeartbeatMessage(
    string ChannelName,
    Device Device,
    SignalTag Tag,
    PayloadData Self,
    bool IsConnected,
    bool IsOnlySign = false) : IMessage;
