namespace ThingsEdge.ServerApp.Configuration;

/// <summary>
/// 应用配置
/// </summary>
public sealed class AppConfig
{
    /// <summary>
    /// 应用程序 Title
    /// </summary>
    [NotNull]
    public string? Title { get; init; } = "IoT Edge Service";
}
