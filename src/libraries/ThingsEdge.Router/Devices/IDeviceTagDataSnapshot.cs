namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备标记快照，用于读取当前的标记值。
/// </summary>
public interface IDeviceTagDataSnapshot
{
    /// <summary>
    /// 获取指定标记的值，没有找到时返回 null。
    /// </summary>
    /// <param name="tagId">标记 Id</param>
    /// <returns></returns>
    PayloadData? Get(string tagId);
}
