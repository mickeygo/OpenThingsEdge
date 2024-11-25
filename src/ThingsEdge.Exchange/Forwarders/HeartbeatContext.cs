using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 心跳传送数据上下文。
/// </summary>
/// <param name="ChannelName">通道名称</param>
/// <param name="Device">设备</param>
/// <param name="Tag">标记</param>
/// <param name="IsOnline">是否在线</param>
public record HeartbeatContext(string ChannelName, Device Device, Tag Tag, bool IsOnline);
