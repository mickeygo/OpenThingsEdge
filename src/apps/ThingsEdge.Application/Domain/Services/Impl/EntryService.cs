namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class EntryService : IEntryService, ITransientDependency
{
    private readonly SqlSugarRepository<SnTransitRecord> _snTransitRecordRepo;
    private readonly SqlSugarRepository<SnTransitRecordLog> _snTransitRecordLogRepo;

    public EntryService(SqlSugarRepository<SnTransitRecord> snTransitRecordRepo,
        SqlSugarRepository<SnTransitRecordLog> snTransitRecordLogRepo)
    {
        _snTransitRecordRepo = snTransitRecordRepo;
        _snTransitRecordLogRepo = snTransitRecordLogRepo;
    }

    public async Task EntryAsync(string line, string station, string sn)
    {
        // 新增过站记录明细
        await _snTransitRecordLogRepo.InsertAsync(new SnTransitRecordLog
        {
            SN = sn,
            Line = line,
            Station = station,
            TransitType = 1, // 进站
            RecordTime = DateTime.Now,
        });

        // 1. 检查SN在该工位的状态
        //  1.1 没有记录 => 新增进站记录
        //  1.2 已进站但还未出站（重复进站）=> 重置进站时间（防止因件搬离工站时间过长导致数据偏差较大）
        //  1.3 有进站和出站记录 => 新增进站记录
        var transitRecord = await _snTransitRecordRepo.GetFirstAsync(s => s.Line == line && s.Station == station && s.SN == sn);
        if (transitRecord is null || transitRecord.IsArchived)
        {
            await _snTransitRecordRepo.InsertAsync(new SnTransitRecord { SN = sn, Line = line, Station = station, EntryTime = DateTime.Now });
            return;
        }

        // 重置进站时间
        transitRecord.EntryTime = DateTime.Now;
        await _snTransitRecordRepo.AsUpdateable(transitRecord).UpdateColumns(s => new { s.EntryTime }).ExecuteCommandAsync();
    }
}
