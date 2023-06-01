namespace ThingsEdge.Common.Utils;

/// <summary>
/// 日期时间帮助类。
/// </summary>
public static class TimeUtil
{
    /// <summary>
    /// 将 <see cref="DateOnly"/> 转换为 <see cref="DateTime"/>, 例如: 2001-02-13 => 2021-02-13 00:00:00.000
    /// </summary>
    /// <param name="dateOnly">要转换的日期</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(DateOnly dateOnly) 
        => new(dateOnly.DayNumber * TimeSpan.TicksPerDay);

    /// <summary>
    /// 将 <see cref="DateOnly"/> 转换为 <see cref="DateTime"/>, 例如: 2001-02-13 => 2021-02-13 23:59:59.999
    /// </summary>
    /// <param name="dateOnly">要转换的日期</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTimeCeil(DateOnly dateOnly)
        => ToDateTime(dateOnly).AddMilliseconds(86_399_999);
}
