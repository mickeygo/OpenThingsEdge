using ThingsEdge.Application.Infrastructure;

namespace ThingsEdge.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 ThingsEdge 应用服务。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddThingsEdgeApplication(this IServiceCollection services)
    {
        services.AddSqlSugarSetup();
        return services;
    }
}
