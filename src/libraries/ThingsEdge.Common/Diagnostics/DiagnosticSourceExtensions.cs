namespace ThingsEdge.Common.Diagnostics;

public static class DiagnosticSourceExtensions
{
    /// <summary>
    /// 检测 DiagnosticListener 中指定的事件名称是否可用，若可用则写入值。
    /// </summary>
    /// <param name="diagnosticSource"></param>
    /// <param name="name">要写入的事件名称。</param>
    /// <param name="value">写入的匿名对象。</param>
    public static void CheckAndWrite(this DiagnosticSource diagnosticSource, string name, object? value)
    {
        if (diagnosticSource.IsEnabled(name))
        {
            diagnosticSource.Write(name, value);
        }
    }
}
