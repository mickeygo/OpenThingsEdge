using ThingsEdge.Communication.Core.Device;

namespace ThingsEdge.Communication.Algorithms.ConnectPool;

/// <summary>
/// 基于设备通信的连接池信息<br />
/// Connection pool information based on device communication
/// </summary>
public class DeviceCommConnector : IConnector
{
    private DeviceCommunication device = null;

    /// <summary>
    /// 获取当前实际的设备通信对象<br />
    /// Gets the actual device communication object at present
    /// </summary>
    public DeviceCommunication Device => device;

    /// <inheritdoc cref="P:HslCommunication.Algorithms.ConnectPool.IConnector.IsConnectUsing" />
    public bool IsConnectUsing { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Algorithms.ConnectPool.IConnector.GuidToken" />
    public string GuidToken { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Algorithms.ConnectPool.IConnector.LastUseTime" />
    public DateTime LastUseTime { get; set; }

    /// <summary>
    /// 使用指定的设备来实例化一个对象<br />
    /// Use the specified device to instantiate an object
    /// </summary>
    /// <param name="device">指定的设备通信</param>
    public DeviceCommConnector(DeviceCommunication device)
    {
        this.device = device;
    }

    /// <inheritdoc cref="M:HslCommunication.Algorithms.ConnectPool.IConnector.Close" />
    public void Close()
    {
        device.CommunicationPipe.CloseCommunication();
    }

    /// <inheritdoc cref="M:HslCommunication.Algorithms.ConnectPool.IConnector.Open" />
    public void Open()
    {
    }
}
