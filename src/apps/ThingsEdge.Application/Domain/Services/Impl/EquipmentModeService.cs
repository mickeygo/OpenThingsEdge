using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Models;

namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class EquipmentModeService : IEquipmentModeService
{
    private readonly SqlSugarRepository<EquipmentModeRecord> _equipModeRepo;

    public EquipmentModeService(SqlSugarRepository<EquipmentModeRecord> equipModeRepo)
    {
        _equipModeRepo = equipModeRepo;
    }

    public async Task<PagedList<EquipmentModeRecord>> GetPagedAsync(PagedQuery query)
    {
        return await _equipModeRepo.AsQueryable()
                .OrderBy(s => s.RecordTime, OrderByType.Desc)
                .ToPagedListAsync(query.PageIndex, query.PageSize);
    }

    public async Task ChangeModeAsync(string line, string equipmentCode, EquipmentRunningMode runningMode)
    {
        await _equipModeRepo.InsertAsync(new EquipmentModeRecord
        {
            Line = line,
            EquipmentCode = equipmentCode,
            EquipmentName = equipmentCode,
            RunningMode = runningMode,
            RecordTime = DateTime.Now,
        });
    }
}
