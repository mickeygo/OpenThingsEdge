namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 用于读写的设备接口，相较于<see cref="T:HslCommunication.Core.IReadWriteDevice" />，额外增加的一个站号信息的属性，参见 <see cref="P:HslCommunication.Core.Net.IReadWriteDeviceStation.Station" />。
public interface IReadWriteDeviceStation : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 获取或设置当前设备站号信息，一般来说，需要在实例化之后设置本站号信息，在通信的时候也可以动态修改当前的站号信息。
    /// </summary>
    byte Station { get; set; }
}
