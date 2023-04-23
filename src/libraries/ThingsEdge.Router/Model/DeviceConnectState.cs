namespace ThingsEdge.Router.Model;

/// <summary>
/// 设备连接状态
/// </summary>
public enum DeviceConnectState
{
    /// <summary>
    /// 在线状态。
    /// </summary>
    Online = 1,

    /// <summary>
    /// 离线状态。
    /// </summary>
    Offline,

    /// <summary>
    /// 未知状态。
    /// </summary>
    Unknown,
}
