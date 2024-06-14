namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 通知数据转发包装类。
/// </summary>
internal sealed class NotificationForwarderWrapper(IServiceProvider serviceProvider) : INotificationForwarderWrapper, ISingletonDependency
{
    public async Task PublishAsync(RequestMessage message, PayloadData? lastMasterPayloadData, CancellationToken cancellationToken = default)
    {
        var keys = ForwarderRegisterHub.Default.Keys;
        if (keys.Length == 0)
        {
            return;
        }

        var forwarders = keys.Select(key => serviceProvider.GetRequiredKeyedService<INotificationForwarder>(key)).ToArray();
        if (forwarders.Length > 0)
        {
            var tasks = forwarders.Select(s => s.PublishAsync(message, lastMasterPayloadData, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false); // 考虑是否能替换为 Task.WhenAny()
        }
    }
}
