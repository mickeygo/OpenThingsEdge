using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class EquipmentModeService : IEquipmentModeService
{
    private readonly SqlSugarRepository<EquipmentModeRecord> _equipModeRepo;

    public EquipmentModeService(SqlSugarRepository<EquipmentModeRecord> equipModeRepo)
    {
        _equipModeRepo = equipModeRepo;
    }

    public async Task ChangeModeAsync(string equipmentCode, EquipmentRunningMode runningMode)
    {
        await _equipModeRepo.InsertAsync(new EquipmentModeRecord
        {
            EquipmentCode = equipmentCode,
            EquipmentName = equipmentCode,
            RunningMode = runningMode,
            RecordTime = DateTime.Now,
        });
    }
}
