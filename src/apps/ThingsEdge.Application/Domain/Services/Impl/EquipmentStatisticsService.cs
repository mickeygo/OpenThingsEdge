namespace ThingsEdge.Application.Domain.Services.Impls;

internal sealed class EquipmentStatisticsService : IEquipmentStatisticsService, ITransientDependency
{
    private readonly SqlSugarRepository<EquipmentStateRecord> _equipStateRepo;
    private readonly SqlSugarRepository<SnTransitRecord> _snTransitRecordRepo;

    public EquipmentStatisticsService(SqlSugarRepository<EquipmentStateRecord> equipStateRepo, 
        SqlSugarRepository<SnTransitRecord> snTransitRecordRepo)
    {
        _equipStateRepo = equipStateRepo;
        _snTransitRecordRepo = snTransitRecordRepo;
    }

    public Task<List<OEEGroupDto>> AnalysisOEEAsync(OEEQueryInput query)
    {
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
        // 产能效率（Performance）= 实际产能 / 标准产能 (合格数+缺陷数)*单件标准耗时 / 实际工作时间)
        // 良率（Yield）= 良品数 / 实际生产数
        // 设备综合效率（OEE）= 稼动率 × 产能效率 × 良率
        // 设备综合生产力（TEEP）= 负荷时间率 × OEE

        // 示例：
        // 设某设备某天工作时间为8h，班前计划停机10min，故障停机30min，设备调整35min，产品的理论加工周期为 1min/件，一天共加工产品400件，有20件废品，求这台设备的OEE
        //  计划运行时间 = 8*60-10=470（min）
        //  实际运行时间 = 470-30-35=405（min）
        //  有效率（稼动率）= 405/470=0.86（86%）
        //  表现性（产能效率）= 400/405=0.98(98%)
        //  质量指数（良率）= (400-20)/400=0.95(95%)
        //  OEE = 有效率*表现性*质量指数=80%

        List<OEEGroupDto> oeeGroups = new();

        // 设备运行状态记录
        var exp = Expressionable.Create<EquipmentStateRecord>();
        exp.Or(s => query.StartTime <= s.StartTime && s.StartTime <= query.EndTime);
        exp.Or(s => s.IsEnded && query.StartTime <= s.EndTime && s.EndTime <= query.EndTime);
        exp.Or(s => s.StartTime <= query.StartTime && ((s.IsEnded && query.EndTime <= s.EndTime) || !s.IsEnded));

        var loadings = _equipStateRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Line), s => s.Line == query.Line)
            .Where(exp.ToExpression())
            .ToList();

        // 生产记录（当前工位已完工的）
        var records = _snTransitRecordRepo.AsQueryable()
            .Where(s => s.EntryTime >= query.StartTime)
            .Where(s => s.IsArchived && s.ArchiveTime <= query.EndTime)
            .ToList();

        // 分组聚合
        var loadingGroup = CalOeeGroup(loadings.Where(s => s.RunningState == EquipmentRunningState.Running), query.StartTime, query.EndTime);
        var warningGroup = CalOeeGroup(loadings.Where(s => s.RunningState == EquipmentRunningState.Warning), query.StartTime, query.EndTime);
        var eStoppingGroup = CalOeeGroup(loadings.Where(s => s.RunningState == EquipmentRunningState.EmergencyStopping), query.StartTime, query.EndTime);
        var recordGroup = records.GroupBy(s => new { s.Line, s.Station })
            .Select(g => new { g.Key.Line, g.Key.Station, TotalCycleTime = g.Sum(g => g.CycleTime), OkCount = g.Count(s => s.IsOK()), NgCount = g.Count(s => s.IsNG()) });

        // 数据计算
        foreach (var loading in loadingGroup)
        {
            OEEDto oee = new()
            {
                EquipmentCode = loading.EquipmentCode,
                LoadingTime = Math.Round(loading.Duration * 1.0 / 60, 2),
            };

            var oeeGroup = oeeGroups.FirstOrDefault(s => s.Line == loading.Line);
            if (oeeGroup is null)
            {
                oeeGroup = new() { Line = loading.Line };
                oeeGroups.Add(oeeGroup);
            }
            oeeGroup.OeeList.Add(oee);

            var warning = warningGroup.FirstOrDefault(s => s.Line == loading.Line && s.EquipmentCode == loading.EquipmentCode);
            if (warning is not null)
            {
                oee.WarningTime = Math.Round(warning.Duration * 1.0 / 60, 2);
            }

            var stopping = eStoppingGroup.FirstOrDefault(s => s.Line == loading.Line && s.EquipmentCode == loading.EquipmentCode);
            if (stopping is not null)
            {
                oee.EStopingTime = Math.Round(stopping.Duration * 1.0 / 60, 2);
            }

            var record0 = recordGroup.FirstOrDefault(s => s.Line == loading.Line && s.Station == loading.EquipmentCode);
            if (record0 is not null)
            {
                oee.WorkingTime = Math.Round(record0.TotalCycleTime * 1.0 / 60, 2);

                if (oee.LoadingTime - oee.EStopingTime > 0)
                {
                    oee.PerformanceRate = Math.Round(oee.WorkingTime / (oee.LoadingTime - oee.EStopingTime), 2);
                }

                oee.OkCount = record0.OkCount;
                oee.NgCount = record0.NgCount;
                oee.TotalCount = oee.OkCount + oee.NgCount;
                if (oee.TotalCount > 0)
                {
                    oee.YieldRate = Math.Round(oee.OkCount * 1.0 / oee.TotalCount, 2);
                }
            }
        }

        // 产线汇总
        foreach (var oeeGroup0 in oeeGroups)
        {
            var totalDuration = oeeGroup0.OeeList.Sum(s => s.LoadingTime - s.EStopingTime);
            var totalCycleTime = oeeGroup0.OeeList.Sum(s => s.WorkingTime);
            if (totalDuration > 0)
            {
                oeeGroup0.AvgPerformanceRate = Math.Round(totalCycleTime / totalDuration, 2);
            }
        }

        return Task.FromResult(oeeGroups);
    }

    /// <summary>
    /// 计算并汇总负荷时长
    /// </summary>
    private static IEnumerable<OeeGroupRecord> CalOeeGroup(IEnumerable<EquipmentStateRecord> records, DateTime start, DateTime end)
    {
        foreach (var s in records)
        {
            if (s.StartTime < start)
            {
                var endTime = s.IsEnded ? s.EndTime!.Value : end;
                s.Duration = Convert.ToInt32((endTime - start).TotalSeconds);
            }
            else if (!s.IsEnded || end < s.EndTime)
            {
                s.Duration = Convert.ToInt32((end - s.StartTime).TotalSeconds);
            }
        }

        return records.GroupBy(s => new { s.Line, s.EquipmentCode })
            .Select(s => new OeeGroupRecord(s.Key.Line, s.Key.EquipmentCode, s.Sum(s => s.Duration)));
    }

    private record class OeeGroupRecord(string Line, string EquipmentCode, int Duration);
}
