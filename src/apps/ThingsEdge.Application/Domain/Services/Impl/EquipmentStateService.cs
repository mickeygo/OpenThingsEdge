using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class EquipmentStateService : IEquipmentStateService
{
    private readonly SqlSugarRepository<EquipmentStateRecord> _equipStateRepo;

    public EquipmentStateService(SqlSugarRepository<EquipmentStateRecord> equipStateRepo)
    {
        _equipStateRepo = equipStateRepo;
    }

    public async Task ChangeStateAsync(string line, string equipmentCode, EquipmentRunningState runningState)
    {
        // 数据运行状态有重叠
        // 运行 => S->运行; E->警报|急停
        // 警报 => S->警报|运行?; E->急停
        // 急停 => S->急停|运行?; E->警报
        // 停止 => E->运行|警报|急停

        switch (runningState)
        {
            case EquipmentRunningState.Running:
                await NewStateAsync(line, equipmentCode, EquipmentRunningState.Running); // 开始运行
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.Warning); // 结束警报
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.EmergencyStopping); // 结束急停
                break;
            case EquipmentRunningState.Warning:
                await NewStateAsync(line, equipmentCode, EquipmentRunningState.Warning); // 开始警报
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.EmergencyStopping); // 结束急停
                break;
            case EquipmentRunningState.EmergencyStopping:
                await NewStateAsync(line, equipmentCode, EquipmentRunningState.EmergencyStopping);
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.Warning);
                break;
            case EquipmentRunningState.Offline:
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.Running); // 结束运行
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.Warning); // 结束警报
                await EndStateAsync(line, equipmentCode, EquipmentRunningState.EmergencyStopping); // 结束急停
                break;
            default:
                break;
        }
    }

    private async Task NewStateAsync(string line, string equipmentCode, EquipmentRunningState runningState)
    {
        // 找到指定运行状态的设备，且设备正处于开启状态
        var equipment = await _equipStateRepo.GetFirstAsync(s => s.Line == line && s.EquipmentCode == equipmentCode && s.RunningState == runningState && !s.IsEnded);
        if (equipment is null)
        {
            await _equipStateRepo.InsertAsync(new EquipmentStateRecord
            {
                Line = line,
                EquipmentCode = equipmentCode,
                EquipmentName = equipmentCode,
                RunningState = runningState,
                StartTime = DateTime.Now,
            });
        }
    }

    private async Task EndStateAsync(string line, string equipmentCode, EquipmentRunningState runningState)
    {
        // 找到指定运行状态的设备，且设备正处于开启状态
        var equipment = await _equipStateRepo.GetFirstAsync(s => s.Line == line && s.EquipmentCode == equipmentCode && s.RunningState == runningState && !s.IsEnded);
        if (equipment is not null)
        {
            equipment.Close();
            await _equipStateRepo.AsUpdateable(equipment).UpdateColumns(s => new
            {
                s.IsEnded,
                s.EndTime,
                s.Duration,
            }).ExecuteCommandAsync();
        }
    }
}
