namespace ThingsEdge.Router.Diagnostics;

/// <summary>
/// ThingsEdge 诊断监听器。
/// </summary>
public sealed class ThingsEdgeDiagnosticListener
{
    public ThingsEdgeDiagnosticListener()
    {
        
    }

    // 发布的消息主题名称
    // 发布的消息参数名称和发布的属性名称要一致
    [DiagnosticName(DiagnosticConstants.WriteLog)]
    public void WriteLog(string formater, string args)
    {
        
    }
}
