namespace ThingsEdge.Application.Domain.Services.Impl;

internal sealed class AlarmSettingService : IAlarmSettingService
{
    const string CacheName = "__AlarmSettingCache";

    private readonly IMemoryCache _memoryCache;
    private readonly SqlSugarRepository<AlarmSetting> _alarmSettingRepo;

    public AlarmSettingService(IMemoryCache memoryCache, SqlSugarRepository<AlarmSetting> alarmSettingRepo)
    {
        _memoryCache = memoryCache;
        _alarmSettingRepo = alarmSettingRepo;

    }

    public async Task<List<AlarmSetting>> GetAllAsync()
    {
        return await _memoryCache.GetOrCreateAsync(CacheName, async cacheEntry =>
        {
            return await _alarmSettingRepo.GetListAsync();
        }) ?? new(0);
    }

    public async Task<AlarmSetting?> GetByIdAsync(long id)
    {
        var alarms = await GetAllAsync();
        return alarms.FirstOrDefault(s => s.Id == id);
    }

    public async Task<List<AlarmSetting>> GetByNoAsync(IEnumerable<int> noList)
    {
        var alarms = await GetAllAsync();
        return alarms.Where(s => noList.Contains(s.No)).ToList();
    }

    public async Task<(bool ok, string? err)> InsertOrUpdateAsync(AlarmSetting alarmSetting)
    {
        var alarms = await GetAllAsync();

        if (alarmSetting.IsTransient()) // 新增
        {
            // 检测是否有相同的编号
            if (alarms.Any(s => s.No == alarmSetting.No))
            {
                return (false, "已存在相同编号的警报");
            }
        }
        else // 更新
        {
            // 检测是否有相同的编号
            if (alarms.Any(s => s.No == alarmSetting.No && s.Id != alarmSetting.Id))
            {
                return (false, "已存在相同编号的警报");
            }
        }

        var ok = await _alarmSettingRepo.InsertOrUpdateAsync(alarmSetting);
        if (ok)
        {
            _memoryCache.Remove(CacheName);
        }

        return (ok, default);
    }

    public async Task DeleteAsync(AlarmSetting alarmSetting)
    {
        var alarms = await GetAllAsync();
        if (!alarms.Any(s => s.Id == alarmSetting.Id))
        {
            return;
        }

        var ok = await _alarmSettingRepo.DeleteAsync(alarmSetting);
        if (ok)
        {
            _memoryCache.Remove(CacheName);
        }
    }
}
