using ThingsEdge.Exchange.Engine.Messages;

namespace ThingsEdge.Exchange.Engine.Handler;

/// <summary>
/// 触发消息处理器。
/// </summary>
internal interface ITriggerMessageHandler : IMessageHandler<TriggerMessage>;
