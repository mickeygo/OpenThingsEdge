using ThingsEdge.Common.EventBus;

namespace ThingsEdge.Common;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 EventBus 事件发布器。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEventBusPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, EventPublisher>();
        return services;
    }
}
