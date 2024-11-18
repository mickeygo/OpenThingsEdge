namespace ThingsEdge.Communication.Exceptions;

/// <summary>
/// Communication 异常对象，框架中的其他异常都会继承此类。
/// </summary>
public class CommunicationException : Exception
{
    public CommunicationException()
    { }

    public CommunicationException(string message) : base(message)
    { }

    public CommunicationException(string message, Exception innerException) : base(message, innerException)
    { }
}
