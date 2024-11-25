using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知传送数据上下文。
/// </summary>
/// <param name="Message">请求消息</param>
/// <param name="LastMasterPayloadData">最近一次记录的标记主数据，没有则为 null</param>
public record NoticeContext(RequestMessage Message, PayloadData? LastMasterPayloadData);
