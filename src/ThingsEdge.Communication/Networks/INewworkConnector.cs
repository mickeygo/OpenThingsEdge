namespace ThingsEdge.Communication.Networks;

/// <summary>
/// 网络连接器接口。
/// </summary>
internal interface INewworkConnector
{
    /// <summary>
    /// 连接指定的网络。
    /// </summary>
    /// <param name="ip">目标 IP 地址</param>
    /// <param name="port">目标端口</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ConnectAsync(string ip, int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="data">要发送的数据。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SendAsync(byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 接收数据。
    /// </summary>
    /// <param name="offset">buffer 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
    /// <param name="count">要从当前流中最多读取的字节数。</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ReceiveAsync(int offset, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 关闭连接。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> CloseAsync(CancellationToken cancellationToken = default);
}
