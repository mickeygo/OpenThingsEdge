namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 通知数据转发包装类。
/// </summary>
internal sealed class NotificationForwarderWrapper(IServiceScopeFactory serviceScopeFactory) : INotificationForwarderWrapper, ISingletonDependency
{
    public async Task PublishAsync(RequestMessage requestMessage, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default)
    {
        var keys = ForwarderRegisterHub.Default.Keys;
        if (keys.Length == 0)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var forwarders = keys.Select(key => scope.ServiceProvider.GetRequiredKeyedService<INotificationForwarder>(key)).ToArray();
        if (forwarders.Length > 0)
        {
            var tasks = forwarders.Select(s => s.PublishAsync(requestMessage, lastMasterPayloadData, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false); // 考虑是否能替换为 Task.WhenAny()
        }
    }
}
