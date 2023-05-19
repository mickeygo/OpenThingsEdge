using ThingsEdge.Application.Domain.Services;
using ThingsEdge.Application.Dtos;
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

    public async Task ChangeModeAsync(EquipmentCodeInput input, EquipmentRunningMode runningMode, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEquipmentModeService>();
        foreach (var equipmentCode in input.EquipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeMode(input.Line, equipmentCode, runningMode);

            // 更新状态记录
            await service.ChangeModeAsync(input.Line, equipmentCode, runningMode);
        }
    }

    public async Task ChangeStateAsync(EquipmentCodeInput input, EquipmentRunningState runningState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEquipmentStateService>();
        foreach (var equipmentCode in input.EquipmentCodes)
        {
            // 更新快照
            _equipmentStateSnapshotManager.ChangeState(input.Line, equipmentCode, runningState);

            // 更新状态记录
            await service.ChangeStateAsync(input.Line, equipmentCode, runningState);
        }
    }
}
