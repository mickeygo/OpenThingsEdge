using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Management.Equipment;

public sealed class EquipmentStateSnapshotManager : IManager
{
    private readonly ConcurrentDictionary<string, EquipmentStateSnapshot> _map = new();

    public EquipmentStateSnapshot? this[string equipmentCode] => GetSnapshot(equipmentCode);

    public EquipmentStateSnapshot? GetSnapshot(string equipmentCode)
    {
        _map.TryGetValue(equipmentCode, out var snapshot);
        return snapshot;
    }

    /// <summary>
    /// 设置设备运行模式。
    /// </summary>
    /// <param name="equipmentCode">设备代码</param>
    /// <param name="runningMode">设备运行模式</param>
    public void ChangeMode(string equipmentCode, EquipmentRunningMode runningMode)
    {
        _map.AddOrUpdate(equipmentCode,
            k =>
            {
                var snapshot0 = new EquipmentStateSnapshot
                {
                    EquipmentCode = equipmentCode,
                    EquipmentName = equipmentCode,
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
    /// <param name="equipmentCode">设备代码</param>
    /// <param name="runningState">设备运行状态</param>
    public void ChangeState(string equipmentCode, EquipmentRunningState runningState)
    {
        _map.AddOrUpdate(equipmentCode,
            k => 
            {
                var snapshot0 = new EquipmentStateSnapshot
                {
                    EquipmentCode = equipmentCode,
                    EquipmentName = equipmentCode,
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
