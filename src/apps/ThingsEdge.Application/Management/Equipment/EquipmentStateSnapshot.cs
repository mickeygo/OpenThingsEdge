namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备状态快照。
/// </summary>
public sealed class EquipmentStateSnapshot
{
    private long _version = 1;

    /// <summary>
    /// 快照更新版本
    /// </summary>
    public long Version => _version;

    /// <summary>
    /// 设备编号
    /// </summary>
    [NotNull]
    public string? EquipmentCode { get; init; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [NotNull]
    public string? EquipmentName { get; init; }

    /// <summary>
    /// 设备运行模式
    /// </summary>
    public EquipmentRunningMode RunningMode { get; private set; } = EquipmentRunningMode.Unkown;

    /// <summary>
    /// 设备运行状态
    /// </summary>
    public EquipmentRunningState RunningState { get; private set; } = EquipmentRunningState.Offline;

    /// <summary>
    /// 设置设备运行模式。
    /// </summary>
    /// <param name="runningMode">设备运行模式</param>
    public void ChangeRunningMode(EquipmentRunningMode runningMode)
    {
        Interlocked.Increment(ref _version);
        RunningMode = runningMode;
    }

    /// <summary>
    /// 设置设备运行状态。
    /// </summary>
    /// <param name="runningState">设备运行状态</param>
    public void ChangeRunningState(EquipmentRunningState runningState)
    {
        Interlocked.Increment(ref _version);
        RunningState = runningState;
    }
}
