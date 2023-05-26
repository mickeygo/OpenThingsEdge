namespace ThingsEdge.ServerApp.ViewModels;

/// <summary>
/// 可观测且带导航的抽象 ViewModel。
/// </summary>
public abstract class AbstractObservableNavViewModel : ObservableObject, INavigationAware
{
    /// <summary>
    /// 是否已执行过初始化操作。
    /// </summary>
    protected bool IsInitialized { get; private set; }

    /// <summary>
    /// 是否每次导航进入时总是执行数据初始化操作，默认 false。
    /// </summary>
    protected bool InitAlways { get; set; }

    /// <summary>
    /// 实现 <see cref="INavigationAware.OnNavigatedTo"/>。
    /// </summary>
    /// <remarks>
    /// 每次导航进入时都会执行此方法，此方法可以做一些数据初始化动作。
    /// </remarks>
    public virtual void OnNavigatedTo()
    {
        if (InitAlways || !IsInitialized)
        {
            Initialize();

            IsInitialized = true;
        }
    }

    /// <summary>
    /// 实现 <see cref="INavigationAware.OnNavigatedFrom"/>
    /// </summary>
    /// <remarks>
    /// 每次导航跳转离开时都会执行此方法，此方法可以做一些数据清理动作。
    /// </remarks>
    public virtual void OnNavigatedFrom()
    {
    }

    /// <summary>
    /// 数据初始化操作，在导航进入时 <see cref="OnNavigatedTo"/> 会调用此方法。
    /// </summary>
    protected virtual void Initialize()
    {
    }
}
