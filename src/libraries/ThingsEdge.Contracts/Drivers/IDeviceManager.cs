namespace ThingsEdge.Contracts;

/// <summary>
/// 设备管理。
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// 获取所有设备信息
    /// </summary>
    /// <returns></returns>
    List<DeviceInfo> GetAll();

    Task<List<DeviceInfo>> GetAllAsync();

    Task<DeviceInfo?> GetAsync(string name);

    Task<(bool ok, string err)> AddAsync(DeviceInfo deviceInfo);

    (bool ok, string err) Update(DeviceInfo deviceInfo);

    void Remove(DeviceInfo deviceInfo);
}
