namespace ThingsEdge.Application.Management.Equipment;

public sealed class EquipmentStateSnapshotManager : ISingletonDependency
{
    record EquipmentStateSnapshotKey(string Line, string EquipmentCode);

    private readonly ConcurrentDictionary<EquipmentStateSnapshotKey, EquipmentStateSnapshot> _map = new();

    public EquipmentStateSnapshot? GetSnapshot(string line, string equipmentCode)
    {
        _map.TryGetValue(new EquipmentStateSnapshotKey(line, equipmentCode), out var snapshot);
        return snapshot;
    }

    /// <summary>
    /// 设置设备运行模式。
    /// </summary>
    /// <param name="line">产线</param>
    /// <param name="equipmentCode">设备代码</param>
    /// <param name="runningMode">设备运行模式</param>
    public void ChangeMode(string line, string equipmentCode, EquipmentRunningMode runningMode)
    {
        _map.AddOrUpdate(new EquipmentStateSnapshotKey(line, equipmentCode),
            k =>
            {
                var snapshot0 = new EquipmentStateSnapshot
                {
                    EquipmentCode = k.EquipmentCode,
                    EquipmentName = k.EquipmentCode,
                };
                snapshot0.ChangeRunningMode(runningMode);

                return snapshot0;
            },
            (_, snapshot) =>
            {
                snapshot.ChangeRunningMode(runningMode);
                return snapshot;
            });
    }

    /// <summary>
    /// 设置设备运行状态。
    /// </summary>
    /// <param name="line">产线</param>
    /// <param name="equipmentCode">设备代码</param>
    /// <param name="runningState">设备运行状态</param>
    public void ChangeState(string line, string equipmentCode, EquipmentRunningState runningState)
    {
        _map.AddOrUpdate(new EquipmentStateSnapshotKey(line, equipmentCode),
            k => 
            {
                var snapshot0 = new EquipmentStateSnapshot
                {
                    EquipmentCode = k.EquipmentCode,
                    EquipmentName = k.EquipmentCode,
                };
                snapshot0.ChangeRunningState(runningState);

                return snapshot0;
            },
            (k, snapshot) =>
            {
                snapshot.ChangeRunningState(runningState);
                return snapshot;
            });
    }
}
