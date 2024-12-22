using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.ConnectionPool;

/// <summary>
/// SocketPool 扩展方法。
/// </summary>
internal static class SocketPoolExtensions
{
    /// <summary>
    /// 获取连接后归还到连接池。
    /// </summary>
    /// <param name="socketPool">连接池</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="SocketException"></exception>
    public static async Task GetAndReturnAsync(this SocketPool socketPool, CancellationToken cancellationToken = default)
    {
        SocketWrapper? socket = null;
        try
        {
            socket = await socketPool.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
        finally
        {
            if (socket != null)
            {
                socketPool.ReleaseConnection(socket);
            }
        }
    }

    /// <summary>
    /// 获取连接并执行，执行结束后归还到连接池。
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="socketPool">连接池</param>
    /// <param name="func">执行方法</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="SocketException"></exception>
    public static async Task<TResult> DoAndReturnAsync<TResult>(this SocketPool socketPool,
        Func<SocketWrapper, Task<TResult>> func)
    {
        SocketWrapper? socket = null;
        try
        {
            socket = await socketPool.GetConnectionAsync().ConfigureAwait(false);
            return await func(socket).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
        finally
        {
            if (socket != null)
            {
                socketPool.ReleaseConnection(socket);
            }
        }
    }

    /// <summary>
    /// 获取连接并执行，执行结束后归还到连接池。
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="socketPool">连接池</param>
    /// <param name="func">执行方法</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="SocketException"></exception>
    public static async Task<TResult> DoAndReturnAsync<TResult>(this SocketPool socketPool,
        Func<SocketWrapper, CancellationToken, Task<TResult>> func,
        CancellationToken cancellationToken)
    {
        SocketWrapper? socket = null;
        try
        {
            socket = await socketPool.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
            return await func(socket, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
        finally
        {
            if (socket != null)
            {
                socketPool.ReleaseConnection(socket);
            }
        }
    }
}
