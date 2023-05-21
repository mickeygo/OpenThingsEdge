using ThingsEdge.Application.Domain.Services;
using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Management.Equipment;

/// <summary>
/// 设备状态管理对象。
/// </summary>
public sealed class EquipmentStateManager
{
    private readonly IEquipmentModeService _equipmentModeService;
    private readonly IEquipmentStateService _equipmentStateService;
    private readonly EquipmentStateSnapshotManager _equipmentStateSnapshotManager;

    public EquipmentStateManager(IEquipmentModeService equipmentModeService,
        IEquipmentStateService equipmentStateService,
        EquipmentStateSnapshotManager equipmentStateSnapshotManager)
    {
        _equipmentModeService = equipmentModeService;
        _equipmentStateService = equipmentStateService;
        _equipmentStateSnapshotManager = equipmentStateSnapshotManager;
    }

    public async Task ChangeModeAsync(EquipmentCodeInput input, EquipmentRunningMode runningMode, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var equipmentCode in input.EquipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeMode(input.Line, equipmentCode, runningMode);

            // 更新状态记录
            await _equipmentModeService.ChangeModeAsync(input.Line, equipmentCode, runningMode);
        }
    }

    public async Task ChangeStateAsync(EquipmentCodeInput input, EquipmentRunningState runningState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var equipmentCode in input.EquipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeState(input.Line, equipmentCode, runningState);

            // 更新状态记录
            await _equipmentStateService.ChangeStateAsync(input.Line, equipmentCode, runningState);
        }
    }
}
