namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 消息轮询器。
/// </summary>
internal interface IMessageLoop
{
    Task LoopAsync(CancellationToken cancellationToken);
}
