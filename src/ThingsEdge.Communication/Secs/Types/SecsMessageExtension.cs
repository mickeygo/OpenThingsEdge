namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// 扩展类
/// </summary>
public static class SecsMessageExtension
{
    /// <summary>
    /// 获取显示的字符串文本信息
    /// </summary>
    /// <param name="secsMessages">SECS消息类</param>
    /// <returns>字符串信息</returns>
    public static string ToRenderString(this SecsValue[] secsMessages)
    {
        if (secsMessages == null || secsMessages.Length == 0)
        {
            return string.Empty;
        }
        var stringBuilder = new StringBuilder();
        foreach (var secsValue in secsMessages)
        {
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(secsValue.ToString());
        }
        return stringBuilder.ToString();
    }
}
