using System.Diagnostics;

namespace ThingsEdge.Communication.Diagnostics;

/// <summary>
/// 诊断适配器
/// </summary>
/// <remarks>
/// 需要在程序启动时将此诊断器加入到诊断侦听器中，参考如下：
/// <code>
/// var diagnosticListener = app.Services.GetRequiredService&lt;DiagnosticListener>();
/// diagnosticListener.SubscribeWithAdapter(new CommunicationDiagnosticListener());
/// </code>
/// </remarks>
public class CommunicationDiagnosticAdapter()
{
    /// <summary>
    /// Debug 写入
    /// </summary>
    /// <param name="diagnosticSource"></param>
    /// <param name="data"></param>
    internal static void Debug(DiagnosticSource diagnosticSource, string data)
    {
        if (diagnosticSource.IsEnabled("ThingsEdge.Communication.Debug"))
        {
            diagnosticSource.Write("ThingsEdge.Communication.Debug", data);
        }
    }
}
