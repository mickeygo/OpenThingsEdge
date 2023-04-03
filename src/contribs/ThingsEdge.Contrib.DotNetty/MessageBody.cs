namespace ThingsEdge.Contrib.DotNetty;

/// <summary>
/// 消息 Body。
/// </summary>
public sealed class MessageBody
{
    public string Message { get; }

    public MessageBody(string message)
    {
        Message = message;
    }

    public string ToJsonFormat() => "{" + $"\"{nameof(MessageBody)}\" :" + "{" + $"\"{nameof(Message)}\"" + " :\"" + Message + "\"}" + "}";
}
