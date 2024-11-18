using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 异形客户端的连接对象
/// </summary>
public class AlienSession
{
    /// <summary>
    /// 网络套接字
    /// </summary>
    public PipeTcpNet Pipe { get; set; }

    /// <summary>
    /// 唯一的标识
    /// </summary>
    public string DTU { get; set; }

    /// <summary>
    /// 密码信息
    /// </summary>
    public string Pwd { get; set; }

    /// <summary>
    /// 指示当前的网络状态
    /// </summary>
    public bool IsStatusOk { get; set; }

    /// <summary>
    /// 上线时间
    /// </summary>
    public DateTime OnlineTime { get; set; }

    /// <summary>
    /// 最后一次下线的时间
    /// </summary>
    public DateTime OfflineTime { get; set; }

    /// <summary>
    /// 实例化一个默认的参数
    /// </summary>
    public AlienSession()
    {
        IsStatusOk = true;
        OnlineTime = DateTime.Now;
        OfflineTime = DateTime.MinValue;
    }

    /// <summary>
    /// 进行下线操作
    /// </summary>
    public void Offline()
    {
        if (IsStatusOk)
        {
            IsStatusOk = false;
            OfflineTime = DateTime.Now;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("DtuSession[" + DTU + "] [" + (IsStatusOk ? "Online" : "Offline") + "]");
        if (IsStatusOk)
        {
            stringBuilder.Append(" [" + SoftBasic.GetTimeSpanDescription(DateTime.Now - OnlineTime) + "]");
        }
        else if (OfflineTime == DateTime.MinValue)
        {
            stringBuilder.Append(" [----]");
        }
        else
        {
            stringBuilder.Append(" [" + SoftBasic.GetTimeSpanDescription(DateTime.Now - OfflineTime) + "]");
        }
        return stringBuilder.ToString();
    }
}
