using ThingsEdge.Common;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Forwarder;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Router;

/// <summary>
/// Extensions for <see cref="IRouterBuilder"/>
/// </summary>
public static class IRouterBuilderExtensions
{
    /// <summary>
    /// 添加要注入到 EventBus 中的程序集。可以注册多个，相同的程序集会进行去重处理。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assembly">要添加的程序集</param>
    /// <returns></returns>
    public static IRouterBuilder AddEventBusRegisterAssembly(this IRouterBuilder builder, Assembly assembly)
    {
        if (!builder.EventAssemblies.Any(x => x.FullName == assembly.FullName))
        {
            builder.EventAssemblies.Add(assembly);
        }

        return builder;
    }

    /// <summary>
    /// 添加设备基于本地文件的提供者。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceFileProvider(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceFactory, DefaultDeviceFactory>();
            services.AddSingleton<IDeviceProvider, FileDeviceProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加自定义设备提供者。
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceCustomProvider<TProvider>(this IRouterBuilder builder)
        where TProvider : class, IDeviceProvider
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceFactory, DefaultDeviceFactory>();
            services.AddSingleton<IDeviceProvider, TProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加设备心跳信息处理服务，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceHeartbeatHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where IHandler : IDeviceHeartbeatApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(IDeviceHeartbeatApi), typeof(IHandler), lifetime));
        });

        return builder;
    }

    /// <summary>
    /// 添加本地的转发处理服务，其中 <see cref="TagFlag.Notice"/> 和 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="lifetime"><see cref="IForwarder"/> 与 <see cref="NativeForwarder"/> 以及 <typeparamref name="TForwarder"/> 注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddNativeForwarder<TForwarder>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TForwarder : INativeForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(INativeForwarder), typeof(TForwarder), lifetime));
            services.Add(new ServiceDescriptor(typeof(IForwarder), ForworderSource.Native.ToString(), typeof(NativeForwarder), lifetime));
            ForwarderRegisterKeys.Default.Register(ForworderSource.Native.ToString());
        });

        return builder;
    }

    /// <summary>
    /// 添加通知消息请求处理服务，其中 <see cref="TagFlag.Notice"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/> 注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddNoticeRequestHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
       where IHandler : INoticePostedApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(INoticePostedApi), typeof(IHandler), lifetime));
        });

        return builder;
    }

    /// <summary>
    /// 添加曲线文件信息处理服务，其中 <see cref="TagFlag.Switch"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="IHandler"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lifetime"><typeparamref name="IHandler"/>注册的生命周期。</param>
    /// <returns></returns>
    public static IRouterBuilder AddCurveFilePostedHandler<IHandler>(this IRouterBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
       where IHandler : ICurveFilePostedApi
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(new ServiceDescriptor(typeof(ICurveFilePostedApi), typeof(IHandler), lifetime));
        });

        return builder;
    }

    /// <summary>
    /// 注册基于 MediatR 的事件总线。
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assemblies">注册事件属于的程序集集合。</param>
    /// <returns></returns>
    public static IRouterBuilder AddEventBus(this IRouterBuilder builder, params Assembly[] assemblies)
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.AddEventBusPublisher(); // 注入 EventBus 事件发布器。

            Assembly[] assemblies2 = assemblies?.Length > 0
                ? [.. builder.EventAssemblies, .. assemblies]
                : [.. builder.EventAssemblies];

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies2));
        });

        return builder;
    }
}
