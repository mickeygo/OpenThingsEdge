using ThingsEdge.Common;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Forwarders;

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
    /// <typeparam name="TDeviceProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceCustomProvider<TDeviceProvider>(this IRouterBuilder builder)
        where TDeviceProvider : class, IDeviceProvider
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDeviceFactory, DefaultDeviceFactory>();
            services.AddSingleton<IDeviceProvider, TDeviceProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 添加设备心跳信息处理服务，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddDeviceHeartbeatForwarder<TForwarder>(this IRouterBuilder builder)
        where TForwarder : INativeHeartbeatForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(INativeHeartbeatForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 添加本地的转发处理服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddNativeRequestForwarder<TForwarder>(this IRouterBuilder builder)
        where TForwarder : IRequestForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IRequestForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 添加通知消息处理服务，其中 <see cref="TagFlag.Notice"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddNativeNoticeForwarder<TForwarder>(this IRouterBuilder builder)
        where TForwarder : INotificationForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            ForwarderRegisterHub.Default.Register("Native");
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(INotificationForwarder), "Native", typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 添加曲线文件信息处理服务，其中 <see cref="TagFlag.Switch"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddCurveFileForwarder<TForwarder>(this IRouterBuilder builder)
       where TForwarder : INativeCurveFileForwarder
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(INativeCurveFileForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 添加本地的转发处理器，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarderHandler"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddNativeRequestForwarderHandler<TForwarderHandler>(this IRouterBuilder builder)
        where TForwarderHandler : IRequestForwarderHandler
    {
        builder.Builder.ConfigureServices(services =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IRequestForwarderHandler), typeof(TForwarderHandler), ServiceLifetime.Transient));
            services.AddSingleton<IRequestForwarder, RequestForwarderProxy>();
        });

        return builder;
    }

    /// <summary>
    /// 注册基于 MediatR 的事件总线。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRouterBuilder AddEventBus(this IRouterBuilder builder)
    {
        builder.Builder.ConfigureServices((hostBuilder, services) =>
        {
            services.AddEventBusPublisher(); // 注入 EventBus 事件发布器。
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies([.. builder.EventAssemblies]));
        });

        return builder;
    }
}
