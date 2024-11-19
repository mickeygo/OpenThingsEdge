using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Profinet.Beckhoff;

/// <summary>
/// Ads设备的相关信息，主要是版本号，设备名称。
/// </summary>
public class AdsDeviceInfo
{
    /// <summary>
    /// 主版本号。
    /// </summary>
    public byte Major { get; set; }

    /// <summary>
    /// 次版本号。
    /// </summary>
    public byte Minor { get; set; }

    /// <summary>
    /// 构建版本号。
    /// </summary>
    public ushort Build { get; set; }

    /// <summary>
    /// 设备的名字。
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// 根据原始的数据内容来实例化一个对象。
    /// </summary>
    /// <param name="data">原始的数据内容</param>
    public AdsDeviceInfo(byte[] data)
    {
        Major = data[0];
        Minor = data[1];
        Build = BitConverter.ToUInt16(data, 2);
        DeviceName = Encoding.ASCII.GetString(data.RemoveBegin(4)).Trim('\0', ' ');
    }
}
