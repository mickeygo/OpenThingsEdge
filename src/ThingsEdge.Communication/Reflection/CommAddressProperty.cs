using System.Reflection;

namespace ThingsEdge.Communication.Reflection;

/// <summary>
/// Hsl相关地址的属性信息
/// </summary>
public class CommAddressProperty
{
    /// <summary>
    /// 该属性绑定的地址特性
    /// </summary>
    public CommDeviceAddressAttribute DeviceAddressAttribute { get; set; }

    /// <summary>
    /// 地址绑定的属性信息
    /// </summary>
    public PropertyInfo PropertyInfo { get; set; }

    /// <summary>
    /// 起始的字节偏移信息
    /// </summary>
    public int ByteOffset { get; set; }

    /// <summary>
    /// 读取的字节的长度信息
    /// </summary>
    public int ByteLength { get; set; }

    /// <summary>
    /// 缓存的数据对象
    /// </summary>
    public byte[] Buffer { get; set; }
}
