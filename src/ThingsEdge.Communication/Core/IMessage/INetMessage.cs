namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 本系统的消息类，包含了各种解析规则，数据信息提取规则。
/// </summary>
public interface INetMessage
{
    /// <summary>
    /// 消息头的指令长度，第一次接受数据的长度。
    /// </summary>
    /// <remarks>
    /// 当最高位字节的最高位为1时，第0和1位为校验的字符数量，第二高位字节表示结束字符之后的剩余字符长度信息，因为一个int占用四个字节，所以最多可以判断2个结束的字符信息。
    /// </remarks>
    int ProtocolHeadBytesLength { get; }

    /// <summary>
    /// 消息头字节。
    /// </summary>
    byte[] HeadBytes { get; set; }

    /// <summary>
    /// 发送的字节信息。
    /// </summary>
    byte[]? SendBytes { get; set; }

    /// <summary>
    /// 从当前的头子节文件中提取出接下来需要接收的数据长度。
    /// </summary>
    /// <remarks>
    /// 如果剩余字节的长度小于0，则表示消息头数据还没有接收完整，还需要接收一定的长度(返回值的绝对值)，
    /// 然后再判断剩余字节长度是否小于0，直到结果大于等于0为止，最多判断的次数为16次，超过16次将返回失败。
    /// </remarks>
    /// <returns>返回接下来的数据内容长度</returns>
    int GetContentLengthByHeadBytes();

    /// <summary>
    /// 在接收头报文的时候，返回前置无效的报文头字节长度，默认为0，不处理。
    /// </summary>
    /// <param name="headByte">接收到的头子节信息</param>
    /// <returns>头子节中无效的字节长度信息</returns>
    int PependedUselesByteLength(byte[] headByte);

    /// <summary>
    /// 检查头子节的合法性。
    /// </summary>
    /// <returns>是否成功的结果</returns>
    bool CheckHeadBytesLegal();

    /// <summary>
    /// 获取头子节里的消息标识。
    /// </summary>
    /// <returns>消息标识</returns>
    int GetHeadBytesIdentity();

    /// <summary>
    /// 当消息头报文的长度定义为-1的时候，则使用动态的长度信息，可以使用本方法来判断一个消息是否处于完整的状态。
    /// </summary>
    /// <param name="send">发送的消息</param>
    /// <param name="ms">接收到的数据内容</param>
    /// <returns>是否是完整的消息</returns>
    bool CheckReceiveDataComplete(byte[] send, MemoryStream ms);

    /// <summary>
    /// 检查发送的接收的报文是否是匹配的，如果匹配，则返回 1,  如果不匹配且直接返回错误，则返回 0，如果不匹配继续接收，直到匹配或是超时，则返回 -1。
    /// </summary>
    /// <remarks>
    /// 在某些协议里，存在一个消息id，发送方的消息id和返回的消息id是必须一致的。
    /// </remarks>
    /// <param name="send">当前发送的报文</param>
    /// <param name="receive">当前接收的报文信息</param>
    /// <returns>如果匹配，则返回 1,  如果不匹配且直接返回错误，则返回 0，如果不匹配继续接收，直到匹配或是超时，则返回 -1</returns>
    int CheckMessageMatch(byte[] send, byte[] receive);
}
