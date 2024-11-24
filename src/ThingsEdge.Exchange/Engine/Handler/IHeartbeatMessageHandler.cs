using ThingsEdge.Exchange.Engine.Messages;

namespace ThingsEdge.Exchange.Engine.Handler;

/// <summary>
/// 心跳消息处理器。
/// </summary>
internal interface IHeartbeatMessageHandler : IMessageHandler<HeartbeatMessage>;
