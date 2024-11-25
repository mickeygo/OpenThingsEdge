using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Workers;

namespace ThingsEdge.Exchange.Engine;

/// <summary>
/// 引擎执行器。
/// </summary>
internal sealed class EngineExecutor(IServiceProvider serviceProvider)
{
    private static readonly HashSet<Type> s_monitorTypes = [];

    /// <summary>
    /// 注册指定的工作器
    /// </summary>
    /// <typeparam name="TWorker"></typeparam>
    public static void Register<TWorker>()
        where TWorker : IWorker
    {
        s_monitorTypes.Add(typeof(TWorker));
    }

    /// <summary>
    /// 监控
    /// </summary>
    /// <param name="connector">驱动连接器</param>
    /// <param name="channelName">通道名称</param>
    /// <param name="device">设备</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        foreach (var monitorType in s_monitorTypes)
        {
            var worker = (IWorker)serviceProvider.GetRequiredService(monitorType);
            await worker.RunAsync(connector, channelName, device, cancellationToken).ConfigureAwait(false);
        }
    }
}
