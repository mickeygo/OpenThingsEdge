namespace ThingsEdge.Router.Events;

/// <summary>
/// 与设备连接的心跳通知事件，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
/// </summary>
/// <param name="ChannelName">通道名称。</param>
/// <param name="Device">Device</param>
/// <param name="Tag">Tag</param>
/// <param name="IsOnline">是否为在线状态。</param>
public sealed record DeviceHeartbeatEvent(string ChannelName, Device Device, Tag Tag, bool IsOnline = false) : INotification;
