using ThingsEdge.Application.Dtos;

namespace ThingsEdge.Application.Domain.Services.Impls;

internal sealed class EquipmentStatisticsService : IEquipmentStatisticsService
{
    private readonly SqlSugarRepository<EquipmentStateRecord> _equipStateRepo;
    private readonly SqlSugarRepository<SnTransitRecord> _snTransitRecordRepo;

    public EquipmentStatisticsService(SqlSugarRepository<EquipmentStateRecord> equipStateRepo, 
        SqlSugarRepository<SnTransitRecord> snTransitRecordRepo)
    {
        _equipStateRepo = equipStateRepo;
        _snTransitRecordRepo = snTransitRecordRepo;
    }

    public async Task<OEEDto> AnalysisOEEAsync(OEEQueryInput query)
    {
        // 运行状态
        var states = await _equipStateRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Line), s => s.Line == query.Line)
            .Where(s => s.StartTime >= query.StartTime)
            .Where(s => (s.IsEnded && s.EndTime <= query.EndTime) || !s.IsEnded)
            .Where(s => s.RunningState == Models.EquipmentRunningState.Running)
            .ToListAsync();

        // 工作记录
        var records = await _snTransitRecordRepo.AsQueryable()
            .Where(s => s.EntryTime >= query.StartTime)
            .Where(s => (s.IsArchived && s.ArchiveTime <= query.EndTime) || !s.IsArchived)
            .ToListAsync();

        // 闭合运行状态和工作状态
        states.ForEach(s =>
        {
            if (!s.IsEnded)
            {
                s.Close();
            }
        });

        records.ForEach(s =>
        {
            if (s.IsArchived)
            {
                s.Archive();
            }
        });

        // 分组聚合
        var stateMap = states.GroupBy(s => s.EquipmentCode)
            .Select(g => new { g.Key, Duration = g.Sum(s => s.Duration) })
            .ToDictionary(k => k.Key, v => v.Duration);
        var recordMap = records.GroupBy(s => s.Station)
            .Select(g => new { g.Key, CycleTime = g.Sum(s => s.CycleTime) })
            .ToDictionary(k => k.Key, v => v.CycleTime);

        foreach (var map in stateMap)
        {
            if (recordMap.TryGetValue(map.Key, out var ct))
            {
                var perc = Math.Round(ct * 1d / map.Value, 2);
            }
        }

        // 基本指标：
        // => 最大操作时间：代表该设施在一定期间内能实际运转的时间，若设备本身可完全由厂内使用，则为日历时间。
        // => 负荷时间（planned Loading Time）：是 TEEP指标的一部分，是设备预期可运转的时间，乃是最大操作时间扣除停机时间，停机时间是计划上的休止时间，如休假、教育训练、保养等。
        // => 稼动率（Availability/Uptime）：是 OEE指标的一部分，是设备实际运转的时间百分比，我们定义稼动时间 等于负荷时间减去停线时间，则稼动率= (稼动时间/负荷时间)*100% 。
        // => 产能效率（Performance）：是 OEE指标的一部分，是实际生产速度和设计生产速度的比例。
        // => 良率（Quality）：也称First Pass Yield（FPY），是 OEE指标的一部分，是有效的良品数和实际生产数的比例。

        // 日历时间 = 工厂运营时间（plant operation time) + 工厂关闭时间（plant closed）
        // 工厂运营时间（plant operation time) = 负荷时间（planned Loading Time）+ 非负荷时间 （planned unLoading Time）
        //  非负荷时间包括 预防性维护、休息、暂停、罢工与不可抗外力停机
        // 稼动时间 = 负荷时间 - 停线时间（非计划内）

        // 负荷时间率（Loading Time）= 工作时间 / 实际可运转时间
        // 稼动率（Availability）= 实际工作时间 / 计划工作时间
        // 产能效率（Performance）= 实际产能 / 标准产能
        // 良率（Yield）= 良品数 / 实际生产数
        // 设备综合效率（OEE）= 稼动率 × 产能效率 × 良率
        // 设备综合生产力（TEEP）= 负荷时间率 × OEE

        // 设某设备某天工作时间为8h，班前计划停机10min，故障停机30min，设备调整35min，产品的理论加工周期为 1min/件，一天共加工产品400件，有20件废品，求这台设备的OEE
        //  计划运行时间 = 8*60-10=470（min）
        //  实际运行时间 = 470-30-35=405（min）
        //  有效率（稼动率）= 405/470=0.86（86%）
        //  表现性（产能效率）= 400/405=0.98(98%)
        //  质量指数（良率）= (400-20)/400=0.95(95%)
        //  OEE = 有效率*表现性*质量指数=80%

        return new OEEDto();
    }
}
