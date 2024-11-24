using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Forwarders;

namespace ThingsEdge.Exchange;

/// <summary>
/// Extensions for <see cref="IExchangeBuilder"/>
/// </summary>
public static class IExchangeBuilderExtensions
{
    /// <summary>
    /// 使用设备基于本地文件的提供者。
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseDeviceFileProvider(this IExchangeBuilder builder)
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton<IAddressFactory, DefaultAddressFactory>();
            services.AddSingleton<IAddressProvider, FileAddressProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 使用自定义设备提供者。
    /// </summary>
    /// <typeparam name="TDeviceProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseDeviceCustomProvider<TDeviceProvider>(this IExchangeBuilder builder)
        where TDeviceProvider : class, IAddressProvider
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton<IAddressFactory, DefaultAddressFactory>();
            services.AddSingleton<IAddressProvider, TDeviceProvider>();
        });
        return builder;
    }

    /// <summary>
    /// 使用设备心跳信息处理服务，其中 <see cref="TagFlag.Heartbeat"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseDeviceHeartbeatForwarder<TForwarder>(this IExchangeBuilder builder)
        where TForwarder : INativeHeartbeatForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(INativeHeartbeatForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 使用本地的转发处理服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseNativeRequestForwarder<TForwarder>(this IExchangeBuilder builder)
        where TForwarder : IRequestForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IRequestForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 使用通知消息处理服务，其中 <see cref="TagFlag.Notice"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseNativeNoticeForwarder<TForwarder>(this IExchangeBuilder builder)
        where TForwarder : INotificationForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            ForwarderRegisterHub.Default.Register("Native");
            services.Add(ServiceDescriptor.DescribeKeyed(typeof(INotificationForwarder), "Native", typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 使用曲线文件信息处理服务，其中 <see cref="TagFlag.Switch"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseCurveFileForwarder<TForwarder>(this IExchangeBuilder builder)
       where TForwarder : INativeCurveFileForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(INativeCurveFileForwarder), typeof(TForwarder), ServiceLifetime.Transient));
        });

        return builder;
    }

    /// <summary>
    /// 使用本地的转发处理器，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarderHandler"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseNativeRequestForwarderHandler<TForwarderHandler>(this IExchangeBuilder builder)
        where TForwarderHandler : IRequestForwarderHandler
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.Add(ServiceDescriptor.Describe(typeof(IRequestForwarderHandler), typeof(TForwarderHandler), ServiceLifetime.Transient));
            services.AddSingleton<IRequestForwarder, RequestForwarderProxy>();
        });

        return builder;
    }
}
