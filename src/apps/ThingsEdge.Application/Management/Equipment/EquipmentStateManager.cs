using ThingsEdge.Application.Domain.Services;
using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备状态管理对象。
/// </summary>
public sealed class EquipmentStateManager : IManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EquipmentStateSnapshotManager _equipmentStateSnapshotManager;

    public EquipmentStateManager(IServiceProvider serviceProvider, EquipmentStateSnapshotManager equipmentStateSnapshotManager)
    {
        _serviceProvider = serviceProvider;
        _equipmentStateSnapshotManager = equipmentStateSnapshotManager;
    }

    public async Task ChangeModeAsync(IEnumerable<string> equipmentCodes, EquipmentRunningMode runningMode, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var service = _serviceProvider.GetRequiredService<IEquipmentModeService>();
        foreach (var equipmentCode in equipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeMode(equipmentCode, runningMode);

            // 更新状态记录
            await service.ChangeModeAsync(equipmentCode, runningMode);
        }
    }

    public async Task ChangeStateAsync(IEnumerable<string> equipmentCodes, EquipmentRunningState runningState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var service = _serviceProvider.GetRequiredService<IEquipmentStateService>();
        foreach (var equipmentCode in equipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeState(equipmentCode, runningState);

            // 更新状态记录
            await service.ChangeStateAsync(equipmentCode, runningState);
        }
    }
}
