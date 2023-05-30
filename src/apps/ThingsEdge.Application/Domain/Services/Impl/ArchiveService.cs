namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class ArchiveService : IArchiveService
{
    private readonly SqlSugarRepository<SnTransitRecord> _snTransitRecordRepo;
    private readonly SqlSugarRepository<SnTransitRecordLog> _snTransitRecordLogRepo;

    public ArchiveService(SqlSugarRepository<SnTransitRecord> snTransitRecordRepo, 
        SqlSugarRepository<SnTransitRecordLog> snTransitRecordLogRepo)
    {
        _snTransitRecordLogRepo = snTransitRecordLogRepo;
        _snTransitRecordRepo = snTransitRecordRepo;
    }

    public async Task ArchiveAsync(string line, string station, string sn)
    {
        // 新增过站记录明细
        await _snTransitRecordLogRepo.InsertAsync(new SnTransitRecordLog
        {
            SN = sn,
            Line = line,
            Station = station,
            TransitType = 2, // 出站
            RecordTime = DateTime.Now,
        });

        // 检查SN在指定工位是否已出站
        var transitRecord = await _snTransitRecordRepo.GetFirstAsync(s => s.Line == line && s.Station == station && s.SN == sn && !s.IsArchived);
        if (transitRecord is not null)
        {
            transitRecord.Archive();
            await _snTransitRecordRepo.AsUpdateable(transitRecord).UpdateColumns(s => new
            {
                s.IsArchived,
                s.ArchiveTime,
                s.CycleTime,
            }).ExecuteCommandAsync();
        }
    }
}
