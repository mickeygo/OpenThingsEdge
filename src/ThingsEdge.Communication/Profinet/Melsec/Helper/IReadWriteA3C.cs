using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱的A3C协议类接口对象，具有站号，是否和校验的属性。
/// </summary>
public interface IReadWriteA3C : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 当前A3C协议的站编号信息。
    /// </summary>
    byte Station { get; set; }

    /// <summary>
    /// 当前的A3C协议是否使用和校验，默认使用。
    /// </summary>
    bool SumCheck { get; set; }

    /// <summary>
    /// 当前的A3C协议的格式信息，可选格式1，2，3，4，默认格式1。
    /// </summary>
    int Format { get; set; }

    /// <summary>
    /// 是否开启支持写入位到字寄存器的功能，该功能先读取字寄存器的字数据，然后修改其中的位，再写入回去，可能存在脏数据的风险。
    /// </summary>
    /// <remarks>
    /// 关于脏数据风险：从读取数据，修改位，再次写入数据时，大概需要经过3ms~10ms不等的时间，
    /// 如果此期间内PLC修改了该字寄存器的其他位，再次写入数据时会恢复该点位的数据到读取时的初始值，可能引发设备故障，请谨慎开启此功能。
    /// </remarks>
    bool EnableWriteBitToWordRegister { get; set; }
}
