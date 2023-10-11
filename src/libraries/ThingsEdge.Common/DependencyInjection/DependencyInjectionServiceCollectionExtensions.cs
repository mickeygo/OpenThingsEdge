namespace ThingsEdge.Common.DependencyInjection;

/// <summary>
/// Common 服务依赖扩展
/// </summary>
public static class DependencyInjectionServiceCollectionExtensions
{
    /// <summary>
    /// 根据指定的 <see cref="ITransientDependency"/>、<see cref="IScopedDependency"/> 和 <see cref="ISingletonDependency"/> 接口注册带有相应生命周期的服务。
    /// </summary>
    /// <remarks>注：若有对象有多个接口，只注册第一个</remarks>
    /// <param name="services">服务</param>
    /// <param name="assembly">检索的程序集</param>
    /// <returns></returns>
    public static IServiceCollection AddAutoDependencyInjection(this IServiceCollection services, Assembly assembly)
    {
        var lifetimeInterfaces = new[] { typeof(ITransientDependency), typeof(IScopedDependency), typeof(ISingletonDependency) };

        var implTypes = assembly.GetTypes()
            .Where(u => lifetimeInterfaces.Any(s => s.IsAssignableFrom(u)) && u.IsClass && !u.IsInterface && !u.IsAbstract);

        foreach (var implType in implTypes)
        {
            var interfaces = implType.GetInterfaces();

            // 获取所有能注册的接口（排除 IDisposable 和 IAsyncDisposable）
            var canInjectInterfaces = interfaces.Where(u => u != typeof(IDisposable)
                            && u != typeof(IAsyncDisposable)
                            && !lifetimeInterfaces.Contains(u)
                            && ((!implType.IsGenericType && !u.IsGenericType)
                                || (implType.IsGenericType && u.IsGenericType && implType.GetGenericArguments().Length == u.GetGenericArguments().Length))
                            );
            
            // 若有多个接口，只注册第一个；若没找到对应接口，表示注册对象本身。
            Type serviceType = canInjectInterfaces.LastOrDefault() ?? implType;

            // 获取生存周期类型
            var dependencyType = interfaces.Last(u => lifetimeInterfaces.Contains(u));
            var lifetime = TryGetServiceLifetime(dependencyType);

            services.TryAdd(ServiceDescriptor.Describe(serviceType, implType, lifetime));
        }

        return services;
    }

    private static ServiceLifetime TryGetServiceLifetime(Type dependencyType)
    {
        if (dependencyType == typeof(ITransientDependency))
        {
            return ServiceLifetime.Transient;
        }
        else if (dependencyType == typeof(IScopedDependency))
        {
            return ServiceLifetime.Scoped;
        }
        else if (dependencyType == typeof(ISingletonDependency))
        {
            return ServiceLifetime.Singleton;
        }
        else
        {
            throw new InvalidCastException("Invalid service registration lifetime.");
        }
    }
}
