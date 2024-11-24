using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Engine.Monitors;

/// <summary>
/// 监控轮询
/// </summary>
internal sealed class MonitorLoop
{
    private static readonly HashSet<Type> s_monitorTypes = [];

    private readonly IServiceProvider _serviceProvider;

    public MonitorLoop(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 注册指定的监控器
    /// </summary>
    /// <typeparam name="TMonitor"></typeparam>
    public static void Register<TMonitor>()
        where TMonitor : AbstractMonitor
    {
        s_monitorTypes.Add(typeof(TMonitor));
    }

    /// <summary>
    /// 监控
    /// </summary>
    /// <param name="connector">驱动连接器</param>
    /// <param name="channelName">通道名称</param>
    /// <param name="device">设备</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        foreach (var monitorType in s_monitorTypes)
        {
            var monitor = (AbstractMonitor)_serviceProvider.GetRequiredService(monitorType);
            monitor.Monitor(connector, channelName, device, cancellationToken);
        }
    }
}
