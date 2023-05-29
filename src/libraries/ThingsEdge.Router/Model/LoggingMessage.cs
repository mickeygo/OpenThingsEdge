namespace ThingsEdge.Router.Model;

/// <summary>
/// 日志记录消息
/// </summary>
public sealed class LoggingMessage
{
    /// <summary>
    /// 日志记录时间。
    /// </summary>
    public DateTime LoggedTime { get; init; }

    /// <summary>
    /// 日志消息级别。
    /// </summary>
    public LoggingLevel Level { get; init; }

    /// <summary>
    /// 日志消息
    /// </summary>
    [NotNull]
    public string? Message { get; init; } = string.Empty;
}
