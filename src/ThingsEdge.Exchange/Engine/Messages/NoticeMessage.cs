using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Messages;

/// <summary>
/// 通知消息。
/// </summary>
/// <param name="Connector">连接驱动</param>
/// <param name="ChannelName">通道名称</param>
/// <param name="Device">设备信息</param>
/// <param name="Tag">标记</param>
/// <param name="Self">读取的标记值</param>
internal record NoticeMessage(
    IDriverConnector Connector,
    string ChannelName,
    Device Device,
    Tag Tag,
    PayloadData Self) : IMessage;
