using ThingsEdge.Exchange.Engine.Messages;

namespace ThingsEdge.Exchange.Engine.Handlers;

/// <summary>
///开关消息处理器。
/// </summary>
internal interface ISwitchMessageHandler : IMessageHandler<SwitchMessage>;
