using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Events;

/// <summary>
/// 日志消息记录事件。
/// </summary>
public sealed class LoggingMessageEvent : INotification
{
    /// <summary>
    /// 事件创建时间。
    /// </summary>
    public DateTime EventTime { get; init; } = DateTime.Now;

    /// <summary>
    /// 日志消息级别。
    /// </summary>
    public LoggingLevel Level { get; init; } = LoggingLevel.Info;

    /// <summary>
    /// 日志消息
    /// </summary>
    [NotNull]
    public string? Message { get; init; } = string.Empty;

    /// <summary>
    /// 通知消息。
    /// </summary>
    /// <param name="message">要记录的消息。</param>
    /// <returns></returns>
    public static LoggingMessageEvent Info(string message)
    {
        return new() { Level = LoggingLevel.Info, Message = message };
    }

    /// <summary>
    /// 警告消息。
    /// </summary>
    /// <param name="message">要记录的消息。</param>
    /// <returns></returns>
    public static LoggingMessageEvent Warning(string message)
    {
        return new() { Level = LoggingLevel.Warning, Message = message };
    }

    /// <summary>
    /// 错误消息。
    /// </summary>
    /// <param name="message">要记录的消息。</param>
    /// <returns></returns>
    public static LoggingMessageEvent Error(string message)
    {
        return new() { Level = LoggingLevel.Error, Message = message };
    }
}
