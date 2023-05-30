namespace ThingsEdge.Application.Configuration;

/// <summary>
/// 应用程序配置信息。
/// </summary>
public sealed class ApplicationConfig
{
    /// <summary>
    /// 标签定义
    /// </summary>
    [NotNull]
    public TagDefine? TagDefine { get; init; } = new();
}
