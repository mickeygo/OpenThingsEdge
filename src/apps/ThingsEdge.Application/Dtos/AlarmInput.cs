namespace ThingsEdge.Application.Dtos;

public sealed class AlarmInput
{
    /// <summary>
    /// 产线
    /// </summary>
    [NotNull]
    public string? Line { get; set; }

    /// <summary>
    /// 上一次的警报集合，没有则为null
    /// </summary>
    public bool[]? LastAlarms { get; set; }

    /// <summary>
    /// 本次产生的警报集合。
    /// </summary>
    [NotNull]
    public bool[]? NewAlarms { get; set;}

    /// <summary>
    /// 地址设置的编号基数是否从 0 开始，默认为 false。
    /// </summary>
    public bool IsSettingNoBaseZero { get; set; }
}
