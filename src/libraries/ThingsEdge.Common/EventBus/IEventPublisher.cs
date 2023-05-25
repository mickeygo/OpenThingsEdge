using MediatR;

namespace ThingsEdge.Common.EventBus;

/// <summary>
/// 事件发布器。
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布事件。
    /// </summary>
    /// <remarks>注：会使用默认的发布策略，即 <see cref="PublishStrategy.SyncContinueOnException"/> 。</remarks>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="notification">要发布的事件。</param>
    /// <returns></returns>
    Task Publish<TNotification>(TNotification notification);

    /// <summary>
    /// 发布事件。
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="notification">要发布的事件。</param>
    /// <param name="strategy">事件的发布策略。</param>
    /// <returns></returns>
    Task Publish<TNotification>(TNotification notification, PublishStrategy strategy);

    /// <summary>
    /// 发布事件。
    /// </summary>
    /// <remarks>注：会使用默认的发布策略，即 <see cref="PublishStrategy.SyncContinueOnException"/> 。</remarks>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="notification">要发布的事件。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken);

    /// <summary>
    /// 发布事件。
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="notification">要发布的事件。</param>
    /// <param name="strategy">事件的发布策略</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken);
}
