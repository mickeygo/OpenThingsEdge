using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// CIP协议的基础接口信息
/// </summary>
public interface IReadWriteCip : IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ByteTransform" />
    IByteTransform ByteTransform { get; set; }

    /// <summary>
    /// 使用指定的类型写入指定的节点数据，类型信息参考API文档，地址支持携带类型代号信息，可以强制指定本次写入数据的类型信息，例如 "type=0xD1;A"<br />
    /// Use the specified type to write the specified node data. For the type information, refer to the API documentation. The address supports carrying type code information. 
    /// You can force the type information of the data to be written this time. For example, "type=0xD1;A"
    /// </summary>
    /// <remarks>
    /// 关于参数 length 的含义，表示的是地址长度，一般的标量数据都是 1，如果PLC有个标签是 A，数据类型为 byte[10]，那我们写入 3 个byte就是 WriteTag( "A[5]", 0xD1, new byte[]{1,2,3}, 3 );<br />
    /// Regarding the meaning of the parameter length, it represents the address length. The general scalar data is 1. If the PLC has a tag of A and the data type is byte[10], then we write 3 bytes as WriteTag( "A[5 ]", 0xD1, new byte[]{1,2,3}, 3 );
    /// </remarks>
    /// <param name="address">节点的名称 -&gt; Name of the node </param>
    /// <param name="typeCode">类型代码，详细参见<see cref="T:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper" />上的常用字段 -&gt;  Type code, see the commonly used Fields section on the <see cref="T:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper" /> in detail</param>
    /// <param name="value">实际的数据值 -&gt; The actual data value </param>
    /// <param name="length">如果节点是数组，就是数组长度 -&gt; If the node is an array, it is the array length </param>
    /// <returns>是否写入成功 -&gt; Whether to write successfully</returns>
    OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.ReadTag(System.String,System.UInt16)" />
    OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice)" />
    OperateResult<string> ReadPlcType();

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />
    Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String)" />
    OperateResult<DateTime> ReadDate(string address);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.DateTime)" />
    OperateResult WriteDate(string address, DateTime date);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteDate(System.String,System.DateTime)" />
    OperateResult WriteTimeAndDate(string address, DateTime date);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadTime(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String)" />
    OperateResult<TimeSpan> ReadTime(string address);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteTime(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.TimeSpan)" />
    OperateResult WriteTime(string address, TimeSpan time);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteTimeOfDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.TimeSpan)" />
    OperateResult WriteTimeOfDate(string address, TimeSpan timeOfDate);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.ReadDate(System.String)" />
    Task<OperateResult<DateTime>> ReadDateAsync(string address);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteDate(System.String,System.DateTime)" />
    Task<OperateResult> WriteDateAsync(string address, DateTime date);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteTimeAndDate(System.String,System.DateTime)" />
    Task<OperateResult> WriteTimeAndDateAsync(string address, DateTime date);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.ReadTime(System.String)" />
    Task<OperateResult<TimeSpan>> ReadTimeAsync(string address);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteTime(System.String,System.TimeSpan)" />
    Task<OperateResult> WriteTimeAsync(string address, TimeSpan time);

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.IReadWriteCip.WriteTimeOfDate(System.String,System.TimeSpan)" />
    Task<OperateResult> WriteTimeOfDateAsync(string address, TimeSpan timeOfDate);
}
