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
    /// 使用设备基于本地文件的提供者，默认执行路径为 "[执行目录]/config/tags.conf"。
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
    /// 使用自定义设备提供者，自定义对象需实现 <see cref="IAddressProvider"/> 接口。
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
        where TForwarder : IHeartbeatForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.AddTransient(typeof(IHeartbeatForwarder), typeof(TForwarder));
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
        where TForwarder : INoticeForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.AddTransient(typeof(INoticeForwarder), typeof(TForwarder));
        });

        return builder;
    }

    /// <summary>
    /// 使用本地的请求处理服务，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
    /// </summary>
    /// <typeparam name="TForwarder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IExchangeBuilder UseNativeTriggerForwarder<TForwarder>(this IExchangeBuilder builder)
        where TForwarder : ITriggerForwarder
    {
        builder.Builder.ConfigureServices((_, services) =>
        {
            services.AddTransient(typeof(ITriggerForwarder), typeof(TForwarder));
        });

        return builder;
    }
}
