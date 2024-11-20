using Microsoft.Extensions.DiagnosticAdapter;

namespace ThingsEdge.Communication.Diagnostics;

/// <summary>
/// 诊断侦听器
/// </summary>
public class CommunicationDiagnosticListener
{
    [DiagnosticName("ThingsEdge.Communication.Debug")]
    public virtual void Debug(string data)
    {
        Console.WriteLine(data);
    }

    [DiagnosticName("ThingsEdge.Communication.Trace")]
    public virtual void Trace(string data)
    {
        Console.WriteLine(data);
    }

    [DiagnosticName("ThingsEdge.Communication.Error")]
    public virtual void Error(Exception ex)
    {
        Console.WriteLine(ex);
    }
}
