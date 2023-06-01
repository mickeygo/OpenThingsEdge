namespace ThingsEdge.Common.DependencyInjection;

public static class DependencyInjectionServiceCollectionExtensions
{
    /// <summary>
    /// 指定注册依赖服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddAutoDependencyInjection(this IServiceCollection services, Assembly assembly)
    {
        var lifetimeInterfaces = new[] { typeof(ITransientDependency), typeof(IScopedDependency), typeof(ISingletonDependency) };

        var types = assembly.GetTypes()
            .Where(u => lifetimeInterfaces.Any(s => s.IsAssignableFrom(u)) && u.IsClass && !u.IsInterface && !u.IsAbstract);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            // 获取所有能注册的接口
            var canInjectInterfaces = interfaces.Where(u => u != typeof(IDisposable)
                            && u != typeof(IAsyncDisposable)
                            && !lifetimeInterfaces.Contains(u)
                            && ((!type.IsGenericType && !u.IsGenericType)
                                || (type.IsGenericType && u.IsGenericType && type.GetGenericArguments().Length == u.GetGenericArguments().Length))
                            ).ToArray();
            
            // 若有多个接口，只注册第一个
            Type? interfaceType = canInjectInterfaces.LastOrDefault();

            // 获取生存周期类型
            var dependencyType = interfaces.Last(u => lifetimeInterfaces.Contains(u));
            var lifetime = TryGetServiceLifetime(dependencyType);

            services.TryAdd(ServiceDescriptor.Describe(interfaceType ?? type, type, lifetime));
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
