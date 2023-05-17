namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 警报记录。
/// </summary>
[SugarTable("AlarmRecord")]
public sealed class AlarmRecord : EntityBase
{
    /// <summary>
    /// 警报定义的编号
    /// </summary>
    public int No { get; set; }

    /// <summary>
    /// 警报信息
    /// </summary>
    [NotNull]
    public string? Message { get; set; }

    /// <summary>
    /// 警报开始时间
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 警报结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 警报是否已关闭。
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// 警报持续时间，单位秒。
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// 关闭警报
    /// </summary>
    public void Close()
    {
        IsClosed = true;
        EndTime = DateTime.Now;
        Duration = (EndTime - StartTime).Value.TotalSeconds;
    }
}
