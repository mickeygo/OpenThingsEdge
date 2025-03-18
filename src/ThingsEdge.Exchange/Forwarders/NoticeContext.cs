using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Forwarders;

/// <summary>
/// 通知传送数据上下文。
/// </summary>
/// <param name="Message">请求消息。</param>
public record NoticeContext(RequestMessage Message);
