namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// Communication 异常类。
/// </summary>
public class CommunicationException : Exception
{
    public CommunicationException(string message) : base(message)
    { }
}
