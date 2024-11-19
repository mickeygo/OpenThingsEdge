using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Robot.YASKAWA;

/// <summary>
/// 安川的报警信息
/// </summary>
public class YRCAlarmItem
{
    /// <summary>
    /// 报警代码
    /// </summary>
    public int AlarmCode { get; set; }

    /// <summary>
    /// 报警发生的时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 报警文字列名称
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 使用原始数据来实例化一个报警的对象。
    /// </summary>
    /// <param name="byteTransform">字节的变换顺序</param>
    /// <param name="content">原始字节数据</param>
    /// <param name="encoding">字符串的编码信息</param>
    public YRCAlarmItem(IByteTransform byteTransform, byte[] content, Encoding encoding)
    {
        AlarmCode = byteTransform.TransInt32(content, 0);
        Time = Convert.ToDateTime(Encoding.ASCII.GetString(content, 16, 16));
        Message = encoding.GetString(content.RemoveBegin(32));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{AlarmCode}] Time:[{Time}] {Message}";
    }
}
