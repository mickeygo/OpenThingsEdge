using ThingsEdge.Router.Model;

namespace ThingsEdge.Router.Events;

/// <summary>
/// 日志消息事件。
/// </summary>
public sealed class MessageLoggedEvent : INotification
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

    public static MessageLoggedEvent Info(string message)
    {
        return new() { Level = LoggingLevel.Info, Message = message };
    }

    public static MessageLoggedEvent Warning(string message)
    {
        return new() { Level = LoggingLevel.Warning, Message = message };
    }

    public static MessageLoggedEvent Error(string message)
    {
        return new() { Level = LoggingLevel.Error, Message = message };
    }
}

