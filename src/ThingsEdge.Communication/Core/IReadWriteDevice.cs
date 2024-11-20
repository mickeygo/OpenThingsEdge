namespace ThingsEdge.Communication.Core;

/// <summary>
/// 用于读写的设备接口，相较于 <see cref="IReadWriteNet" />，增加了 <see cref="ReadFromCoreServerAsync(byte[])" /> 相关的方法，可以用来和设备进行额外的交互。
/// </summary>
public interface IReadWriteDevice : IReadWriteNet
{
    /// <summary>
    /// 当前的数据变换机制，当你需要从字节数据转换类型数据的时候需要。
    /// </summary>
    /// <remarks>
    /// 提供了三种数据变换机制，分别是 <see cref="RegularByteTransform" />, <see cref="ReverseBytesTransform" />, <see cref="ReverseWordTransform" />，
    /// 各自的<see cref="DataFormat" />属性也可以自定调整，基本满足所有的情况使用。
    /// </remarks>
    IByteTransform ByteTransform { get; }

    /// <summary>
    /// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。
    /// </summary>
    /// <param name="send">发送的完整的报文信息</param>
    /// <returns>接收的完整的报文信息</returns>
    /// <remarks>
    /// 本方法用于实现本组件还未实现的一些报文功能，例如有些modbus服务器会有一些特殊的功能码支持，需要收发特殊的报文。
    /// </remarks>
    Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send);

    /// <summary>
    /// 将多个数据报文按顺序发到设备，并从设备接收返回的数据内容，然后拼接成一个Byte[]信息，
    /// 需要重写相关的 UnpackResponseContent 方法才能返回正确的结果。
    /// </summary>
    /// <param name="sends">发送的报文列表信息</param>
    /// <returns>是否接收成功</returns>
    Task<OperateResult<byte[]>> ReadFromCoreServerAsync(IEnumerable<byte[]> sends);
}
