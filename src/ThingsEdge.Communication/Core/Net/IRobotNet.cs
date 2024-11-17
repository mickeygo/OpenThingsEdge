using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 所有的机器人的统一读写标准，统一的基本的读写操作。
/// </summary>
public interface IRobotNet
{
    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkBase.LogNet" />
    ILogNet LogNet { get; set; }

    /// <summary>
    /// 根据地址读取机器人的原始的字节数据信息<br />
    /// Read the robot's original byte data information according to the address
    /// </summary>
    /// <param name="address">指定的地址信息，对于某些机器人无效</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    OperateResult<byte[]> Read(string address);

    /// <summary>
    /// 根据地址读取机器人的字符串的数据信息<br />
    /// Read the string data information of the robot based on the address
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的字符串数据</returns>
    OperateResult<string> ReadString(string address);

    /// <summary>
    /// 根据地址，来写入设备的相关的字节数据<br />
    /// According to the address, to write the device related bytes data
    /// </summary>
    /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
    /// <param name="value">原始的字节数据信息</param>
    /// <returns>是否成功的写入</returns>
    OperateResult Write(string address, byte[] value);

    /// <summary>
    /// 根据地址，来写入设备相关的字符串数据<br />
    /// According to the address, to write the device related string data
    /// </summary>
    /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
    /// <param name="value">字符串的数据信息</param>
    /// <returns>是否成功的写入</returns>
    OperateResult Write(string address, string value);

    /// <summary>
    /// 根据地址读取机器人的原始的字节数据信息。
    /// </summary>
    /// <param name="address">指定的地址信息，对于某些机器人无效</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    Task<OperateResult<byte[]>> ReadAsync(string address);

    /// <summary>
    /// 根据地址读取机器人的字符串的数据信息。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的字符串数据</returns>
    Task<OperateResult<string>> ReadStringAsync(string address);

    /// <summary>
    /// 根据地址，来写入设备的相关的字节数据。
    /// </summary>
    /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
    /// <param name="value">原始的字节数据信息</param>
    /// <returns>是否成功的写入</returns>
    Task<OperateResult> WriteAsync(string address, byte[] value);

    /// <summary>
    /// 根据地址，来写入设备相关的字符串数据。
    /// </summary>
    /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
    /// <param name="value">字符串的数据信息</param>
    /// <returns>是否成功的写入</returns>
    Task<OperateResult> WriteAsync(string address, string value);
}
