namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class AlarmService : IAlarmService, IDomainService
{
    internal static bool[] LatestAlarms = Array.Empty<bool>(); // 上一次警报数据

    private readonly SqlSugarRepository<AlarmRecord> _alarmRecordRepo;
    private readonly IAlarmSettingService _alarmSettingService;
    private readonly ILogger _logger;

    public AlarmService(SqlSugarRepository<AlarmRecord> alarmRecordRepo,
        IAlarmSettingService alarmSettingService,
        ILogger<AlarmService> logger)
    {
        _alarmRecordRepo = alarmRecordRepo;
        _alarmSettingService = alarmSettingService;
        _logger = logger;
    }

    public async Task<(bool ok, string? err)> RecordAlarmsAsync(bool[] alarms)
    {
        // 对当前警报集合和上一次的警报集合进行对比:
        //  0/1 => 新增警报
        //  1/1 => 不变，表示警报持续中
        //  1/0 => 警报结束，关闭警报

        // 新增警报，先从数据库中查找是否该警报已创建且未关闭（防止因服务重启上一次警报记录丢失导致重复创建）

        List<int> newAlarms = new(), closedAlarms = new();

        // 第一次警报
        if (LatestAlarms.Length == 0)
        {
            for (int i = 0; i < alarms.Length; i++)
            {
                if (alarms[i])
                {
                    newAlarms.Add(i + 1); // 基数从1开始
                }
            }
        }
        else
        {
            for (int i = 0; i < alarms.Length; i++)
            {
                bool oldAlarm = LatestAlarms[i], newAlarm = alarms[i]; // 警报数量长度一致
                if (oldAlarm == newAlarm)
                {
                    continue;
                }

                if (!oldAlarm && newAlarm)
                {
                    newAlarms.Add(i + 1);
                }
                else if (oldAlarm && !newAlarm)
                {
                    closedAlarms.Add(i + 1);
                }
            }
        }

        try
        {
            // 新警报先在数据库中进行去重
            if (newAlarms.Any())
            {
                // 检查设置中是否已配置
                var alarmSettings = await _alarmSettingService.GetByNoAsync(newAlarms);

                // 已创建且未关闭的警报
                List<int> newAlarmsNo = alarmSettings.Select(s => s.No).ToList();
                var closingAlarms2 = await _alarmRecordRepo.GetListAsync(s => newAlarmsNo.Contains(s.No) && !s.IsClosed);

                // 排除相应的已创建且未关闭的警报
                var alarms0 = alarmSettings.Where(s => !closingAlarms2.Any(t => t.No == s.No))
                    .Select(s => new AlarmRecord
                    {
                        No = s.No,
                        Message = s.Message,
                        StartTime = DateTime.Now,

                    }).ToList();

                if (alarms0.Any())
                {
                    await _alarmRecordRepo.InsertRangeAsync(alarms0);
                }
            }

            // 关闭警报
            if (closedAlarms.Any())
            {
                // 待关闭的警报
                var closingAlarms = await _alarmRecordRepo.GetListAsync(s => closedAlarms.Contains(s.No) && !s.IsClosed);
                closingAlarms.ForEach(s =>
                {
                    s.Close();
                });
                await _alarmRecordRepo.AsUpdateable(closingAlarms).UpdateColumns(it => new
                {
                    it.IsClosed,
                    it.EndTime,
                    it.Duration,
                    it.UpdateTime,
                }).ExecuteCommandAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AlarmService] 记录警报信息出错");
            return (false, ex.Message);
        }

        LatestAlarms = alarms;

        return (true, default);
    }
}
