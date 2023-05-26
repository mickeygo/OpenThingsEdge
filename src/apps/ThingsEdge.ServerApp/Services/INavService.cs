namespace ThingsEdge.ServerApp.Services;

/// <summary>
/// 导航服务
/// </summary>
public interface INavService
{
    /// <summary>
    /// 导航项
    /// </summary>
    IEnumerable<INavigationControl> NavigationItems { get; }

    /// <summary>
    /// Footer 导航项
    /// </summary>
    IEnumerable<INavigationControl> NavigationFooter { get; }
}
