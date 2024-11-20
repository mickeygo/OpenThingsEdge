using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// CIP协议的基础接口信息
/// </summary>
public interface IReadWriteCip : IReadWriteNet
{
    IByteTransform ByteTransform { get; }

    Task<OperateResult<string>> ReadPlcTypeAsync();

    Task<OperateResult<ushort, byte[]>> ReadTagAsync(string address, ushort length = 1);

    /// <summary>
    /// 使用指定的类型写入指定的节点数据，类型信息参考API文档，地址支持携带类型代号信息，可以强制指定本次写入数据的类型信息，例如 "type=0xD1;A"。
    /// </summary>
    /// <remarks>
    /// 关于参数 length 的含义，表示的是地址长度，一般的标量数据都是 1，如果PLC有个标签是 A，数据类型为 byte[10]，那我们写入 3 个byte就是 WriteTag( "A[5]", 0xD1, new byte[]{1,2,3}, 3 )。
    /// </remarks>
    /// <param name="address">节点的名称</param>
    /// <param name="typeCode">类型代码</param>
    /// <param name="value">实际的数据值</param>
    /// <param name="length">如果节点是数组，就是数组长度</param>
    /// <returns>是否写入成功</returns>
    Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1);

    Task<OperateResult<DateTime>> ReadDateAsync(string address);

    Task<OperateResult> WriteDateAsync(string address, DateTime date);

    Task<OperateResult> WriteTimeAndDateAsync(string address, DateTime date);

    Task<OperateResult<TimeSpan>> ReadTimeAsync(string address);

    Task<OperateResult> WriteTimeAsync(string address, TimeSpan time);

    Task<OperateResult> WriteTimeOfDateAsync(string address, TimeSpan timeOfDate);
}
