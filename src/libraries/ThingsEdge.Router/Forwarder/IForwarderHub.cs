namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 内部注册的 Forwarder 对象服务集合。
/// </summary>
/// <remarks>采用 <see cref="ActivatorUtilities.GetServiceOrCreateInstance"/> 获取服务对象。</remarks>
public interface IForwarderHub
{
    /// <summary>
    /// 注册实例对象类型。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void Register<T>() where T : IForwarder;

    /// <summary>
    /// 移除指定的注册对象类型。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void Remove<T>() where T : IForwarder;

    /// <summary>
    /// 解析所有已注册的实例对象类型。
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<IForwarder> ResloveAll(IServiceProvider serviceProvider);
}
