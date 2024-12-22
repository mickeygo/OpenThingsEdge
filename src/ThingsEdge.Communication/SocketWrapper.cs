using System.Net.Sockets;

namespace ThingsEdge.Communication;

/// <summary>
/// <see cref="Socket"/> 包装类。
/// </summary>
internal sealed class SocketWrapper(Socket socket) : IDisposable
{
    private bool _isClosed;

    /// <summary>
    /// 唯一 Id
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 是否正在使用。
    /// </summary>
    public bool InUse { get; set; }

    /// <summary>
    /// 最近一次使用时间。
    /// </summary>
    public DateTime LastUsedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 获取 Socket 状态是否可用。
    /// </summary>
    public bool Available { get; private set; } = true;

    /// <summary>
    /// 发送数据。
    /// </summary>
    /// <param name="buffer">要发送的数据。</param>
    /// <returns></returns>
    public Task<int> SendAsync(ArraySegment<byte> buffer)
    {
        return socket.SendAsync(buffer);
    }

    /// <summary>
    /// 接受数据。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
    {
        return socket.ReceiveAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// 是否过期。
    /// </summary>
    /// <remarks>当连接超过指定时长未被使用时，表示已过期。</remarks>
    /// <param name="timeout">过期时长。</param>
    /// <returns></returns>
    public bool IsExpired(TimeSpan timeout)
    {
        return DateTime.Now - LastUsedTime > timeout;
    }

    /// <summary>
    /// 关闭连接并标记为不可用状态。
    /// </summary>
    public void Close()
    {
        Available = false;

        if (!_isClosed)
        {
            socket.SafeClose();
            _isClosed = true;
        }
    }

    public void Dispose()
    {
        Close();
    }
}
