namespace ThingsEdge.Communication.Core;

/// <summary>
/// 所有的和设备或是交互类统一读写标准，公开了如何读写对方的一些api接口，并支持基于特性的读写操作。
/// </summary>
/// <remarks>
/// Modbus 类和 PLC 类均实现了本接口，可以基于本接口实现统一所有的不同种类的设备的数据交互。
/// </remarks>
public interface IReadWriteNet
{
    /// <summary>
    /// 组件的日志工具。
    /// </summary>
    ILogger? Logger { get; set; }

    /// <summary>
    /// 当前连接的唯一ID号，默认为长度20的guid码加随机数组成，方便列表管理，也可以自己指定。
    /// </summary>
    /// <remarks>
    /// 当前的唯一连接ID信息在DTU的模式下，将发挥关键作用，标记唯一的DTU ID
    /// </remarks>
    string ConnectionId { get; }

    #region read

    /// <summary>
    /// 异步批量读取字节数组信息，需要指定地址和长度，返回原始的字节数组。
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    Task<OperateResult<byte[]>> ReadAsync(string address, ushort length);

    /// <summary>
    /// 异步读取单个的<see cref="bool" />数据信息。
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    Task<OperateResult<bool>> ReadBoolAsync(string address);

    /// <summary>
    /// 异步批量读取<see cref="bool" />数组信息，需要指定地址和长度，返回<see cref="bool" /> 数组。
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length);

    /// <summary>
    /// 异步读取16位的有符号的整型数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的short数据</returns>
    Task<OperateResult<short>> ReadInt16Async(string address);

    /// <summary>
    /// 异步读取16位的有符号整型数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取的数组长度</param>
    /// <returns>带有成功标识的short数组</returns>
    Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length);

    /// <summary>
    /// 异步读取16位的无符号整型。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的ushort数据</returns>
    Task<OperateResult<ushort>> ReadUInt16Async(string address);

    /// <summary>
    /// 异步读取16位的无符号整型数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取的数组长度</param>
    /// <returns>带有成功标识的ushort数组</returns>
    Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length);

    /// <summary>
    /// 异步读取32位的有符号整型。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的int数据</returns>
    Task<OperateResult<int>> ReadInt32Async(string address);

    /// <summary>
    /// 异步读取32位有符号整型数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的int数组</returns>
    Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length);

    /// <summary>
    /// 异步读取32位的无符号整型。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的uint数据</returns>
    Task<OperateResult<uint>> ReadUInt32Async(string address);

    /// <summary>
    /// 异步读取32位的无符号整型数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的uint数组</returns>
    Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length);

    /// <summary>
    /// 异步读取单浮点数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的float数据</returns>
    Task<OperateResult<float>> ReadFloatAsync(string address);

    /// <summary>
    /// 异步读取单浮点精度的数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的float数组</returns>
    Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length);

    /// <summary>
    /// 异步读取64位的有符号整型。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的long数据</returns>
    Task<OperateResult<long>> ReadInt64Async(string address);

    /// <summary>
    /// 异步读取64位的有符号整型数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的long数组</returns>
    Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length);

    /// <summary>
    /// 异步读取64位的无符号整型。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的ulong数据</returns>
    Task<OperateResult<ulong>> ReadUInt64Async(string address);

    /// <summary>
    /// 异步读取64位的无符号整型的数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的ulong数组</returns>
    Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length);

    /// <summary>
    /// 异步读取双浮点的数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的double数据</returns>
    Task<OperateResult<double>> ReadDoubleAsync(string address);

    /// <summary>
    /// 异步读取双浮点数据的数组。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数组长度</param>
    /// <returns>带有成功标识的double数组</returns>
    Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length);

    /// <summary>
    /// 异步读取字符串数据，默认为最常见的ASCII编码。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的string数据</returns>
    Task<OperateResult<string>> ReadStringAsync(string address, ushort length);

    /// <summary>
    /// 异步使用指定的编码，读取字符串数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <param name="encoding">指定的自定义的编码</param>
    /// <returns>带有成功标识的string数据</returns>
    Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding);

    #endregion

    #region wirte

    /// <summary>
    /// 异步写入原始的byte数组数据到指定的地址，返回是否写入成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="data">要写入的数据</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, byte[] data);

    /// <summary>
    /// 异步批量写入 <see cref="bool" /> 数组数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, bool value);

    /// <summary>
    /// 异步批量写入<see cref="bool" />数组数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, bool[] values);

    /// <summary>
    /// 异步写入short数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, short value);

    /// <summary>
    /// 异步写入short数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, short[] values);

    /// <summary>
    /// 异步写入ushort数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, ushort value);

    /// <summary>
    /// 异步写入ushort数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, ushort[] values);

    /// <summary>
    /// 异步写入int数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, int value);

    /// <summary>
    /// 异步写入int[]数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, int[] values);

    /// <summary>
    /// 异步写入uint数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, uint value);

    /// <summary>
    /// 异步写入uint[]数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, uint[] values);

    /// <summary>
    /// 异步写入float数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, float value);

    /// <summary>
    /// 异步写入float数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, float[] values);

    /// <summary>
    /// 异步写入long数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, long value);

    /// <summary>
    /// 异步写入long数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, long[] values);

    /// <summary>
    /// 异步写入ulong数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, ulong value);

    /// <summary>
    /// 异步写入ulong数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, ulong[] values);

    /// <summary>
    /// 异步写入double数据，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, double value);

    /// <summary>
    /// 异步写入double数组，返回是否成功。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, double[] values);

    /// <summary>
    /// 异步写入字符串信息，编码为ASCII。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, string value);

    /// <summary>
    /// 异步写入字符串信息，需要指定的编码信息。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <param name="encoding">指定的编码信息</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, string value, Encoding encoding);

    /// <summary>
    /// 异步写入指定长度的字符串信息，如果超出，就截断字符串，如果长度不足，那就补0操作，编码为ASCII。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <param name="length">字符串的长度</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, string value, int length);

    /// <summary>
    /// 异步写入指定长度的字符串信息，如果超出，就截断字符串，如果长度不足，那就补0操作，编码为指定的编码信息。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">写入值</param>
    /// <param name="length">字符串的长度</param>
    /// <param name="encoding">指定的编码信息</param>
    /// <returns>带有成功标识的结果类对象</returns>
    Task<OperateResult> WriteAsync(string address, string value, int length, Encoding encoding);

    #endregion
}
