using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 基于网络的通信管道基类。
/// </summary>
 public abstract class NetworkPipeBase : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// 获取或设置接收服务器反馈的时间，如果为负数，则不接收反馈，默认 5s。
    /// </summary>
    public int ReceiveTimeout { get; set; } = 5_000;

    /// <summary>
    /// 获取当前实例的异步锁对象。
    /// </summary>
    public ICommAsyncLock Lock { get; } = new CommAsyncLock();

    /// <summary>
    /// 接收固定长度的字节数组，允许指定超时时间，默认为60秒，当length大于0时，接收固定长度的数据内容，当length小于0时，buffer长度的缓存数据。
    /// </summary>
    /// <param name="buffer">等待接收的数据缓存信息</param>
    /// <param name="offset">开始接收数据的偏移地址</param>
    /// <param name="length">准备接收的数据长度，当length大于0时，接收固定长度的数据内容，当length小于0时，接收不大于2048长度的随机数据信息</param>
    /// <param name="timeout">超时时间，单位：毫秒</param>
    /// <returns>包含了字节数据的结果类</returns>
    public virtual async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeout)
    {
        return await Task.FromResult(new OperateResult<int>(StringResources.Language.NotSupportedFunction)).ConfigureAwait(false);
    }

    public virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(INetMessage? netMessage, byte[] sendValue, bool hasResponseData)
    {
        return await ReadFromCoreServerHelperAsync(netMessage, sendValue, hasResponseData).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送数据到当前的管道中去。
    /// </summary>
    /// <param name="data">要发送的数据</param>
    /// <returns>是否发送成功</returns>
    public abstract Task<OperateResult> SendAsync(byte[] data);

    /// <summary>
    /// 从管道里，接收指定长度的报文数据信息，如果长度指定为-1，表示接收不超过2048字节的动态长度。
    /// </summary>
    /// <param name="length">接收的长度信息</param>
    /// <param name="timeout">指定的超时时间</param>
    /// <returns>是否接收成功的结果对象</returns>
    public virtual async Task<OperateResult<byte[]>> ReceiveAsync(int length, int timeout)
    {
        if (length == 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        var num = length > 0 ? length : 2048;
        var buffer = new byte[num];
        var receive = await ReceiveAsync(buffer, 0, length, timeout).ConfigureAwait(false);
        if (!receive.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(receive);
        }
        return OperateResult.CreateSuccessResult(length > 0 ? buffer : buffer.SelectBegin(receive.Content));
    }

    /// <summary>
    /// 创建并连接（打开）一个新的管道连接对象。
    /// </summary>
    /// <returns>是否打开成功的结果对象</returns>
    public virtual Task<OperateResult<bool>> CreateAndConnectPipeAsync()
    {
        return Task.FromResult(new OperateResult<bool>(StringResources.Language.NotSupportedFunction));
    }

    /// <summary>
    /// 关闭当前的管道信息，返回是否关闭成功的结果对象。
    /// </summary>
    /// <returns>是否关闭成功</returns>
    public virtual OperateResult ClosePipe()
    {
        return new OperateResult(StringResources.Language.NotSupportedFunction);
    }

    private async Task<OperateResult<byte[]>> ReceiveCommandLineFromPipeAsync(byte endCode, int timeout = 60000)
    {
        // HACK: 此代码需要优化
        try
        {
            var bufferArray = new List<byte>(128);
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                var headResult = await ReceiveAsync(1, timeout).ConfigureAwait(continueOnCapturedContext: false);
                if (!headResult.IsSuccess)
                {
                    return headResult;
                }
                bufferArray.AddRange(headResult.Content);
                if (headResult.Content[0] == endCode)
                {
                    bOK = true;
                    break;
                }
            }
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + timeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    public virtual async Task<OperateResult<byte[]>> ReceiveMessageAsync(INetMessage? netMessage, byte[] sendValue)
    {
        // HACK: 代码优化

        OperateResult<byte[]> read;
        if (netMessage == null || netMessage.ProtocolHeadBytesLength == -1)
        {
            if (netMessage != null && netMessage.SendBytes == null)
            {
                netMessage.SendBytes = sendValue;
            }

            var startTime = DateTime.Now;
            var ms = new MemoryStream();
            do
            {
                read = await ReceiveByMessageAsync(ReceiveTimeout, null).ConfigureAwait(false);
                if (!read.IsSuccess)
                {
                    return read;
                }
                if (read.Content != null && read.Content.Length != 0)
                {
                    ms.Write(read.Content);
                }
                if (netMessage == null)
                {
                    return OperateResult.CreateSuccessResult(ms.ToArray());
                }
                if (netMessage.CheckReceiveDataComplete(sendValue, ms))
                {
                    return OperateResult.CreateSuccessResult(ms.ToArray());
                }
            }
            while (ReceiveTimeout < 0 || !((DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeout));
            return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + ReceiveTimeout + " Received: " + ms.ToArray().ToHexString(' '));
        }
        read = await ReceiveByMessageAsync(ReceiveTimeout, netMessage).ConfigureAwait(false);
        return read;
    }

    protected async Task<OperateResult<byte[]>> ReadFromCoreServerHelperAsync(INetMessage? netMessage, byte[] sendValue, bool hasResponseData)
    {
        if (netMessage != null)
        {
            netMessage.SendBytes = sendValue;
        }

        var sendResult = await SendAsync(sendValue).ConfigureAwait(continueOnCapturedContext: false);
        if (!sendResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(sendResult);
        }
        if (ReceiveTimeout < 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        if (!hasResponseData)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }

        var start = DateTime.Now;
        var times = 0;
        OperateResult<byte[]> resultReceive;
        while (true)
        {
            resultReceive = await ReceiveMessageAsync(netMessage, sendValue).ConfigureAwait(false);
            if (!resultReceive.IsSuccess)
            {
                return resultReceive;
            }

            bool num;
            if (netMessage != null)
            {
                switch (netMessage.CheckMessageMatch(sendValue, resultReceive.Content))
                {
                    case 0:
                        return new OperateResult<byte[]>("INetMessage.CheckMessageMatch failed" + Environment.NewLine
                            + StringResources.Language.Send
                            + ": " + sendValue.RemoveBegin(' ')
                            + Environment.NewLine
                            + StringResources.Language.Receive
                            + ": " + resultReceive.Content.ToHexString(' '));
                    case 1:
                        break;
                    default:
                        times++;
                        num = ReceiveTimeout >= 0 && (DateTime.Now - start).TotalMilliseconds > ReceiveTimeout;
                        if (num)
                        {
                            return new OperateResult<byte[]>("Receive Message timeout: " + ReceiveTimeout + " CheckMessageMatch times:" + times);
                        }
                        else
                        {
                            continue;
                        }
                }
            }
            break;
        }
        if (netMessage != null && !netMessage.CheckHeadBytesLegal())
        {
            return new OperateResult<byte[]>((int)CommErrorCode.CommandHeadCodeCheckFailed, StringResources.Language.CommandHeadCodeCheckFailed + Environment.NewLine
                + StringResources.Language.Send + ": " + sendValue.RemoveBegin(' ') + Environment.NewLine
                + StringResources.Language.Receive + ": " + resultReceive.Content.ToHexString(' '));
        }
        return OperateResult.CreateSuccessResult(resultReceive.Content);
    }

    private async Task<OperateResult<byte[]>> ReceiveCommandLineFromPipeAsync(byte endCode1, byte endCode2, int timeout = 60000)
    {
        try
        {
            var bufferArray = new List<byte>(128);
            var st = DateTime.Now;
            var bOK = false;
            while ((DateTime.Now - st).TotalMilliseconds < timeout)
            {
                var headResult = await ReceiveAsync(1, timeout).ConfigureAwait(false);
                if (!headResult.IsSuccess)
                {
                    return headResult;
                }

                bufferArray.AddRange(headResult.Content);
                if (headResult.Content![0] == endCode2 && bufferArray.Count > 1 && bufferArray[^2] == endCode1)
                {
                    bOK = true;
                    break;
                }
            }
            if (!bOK)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + timeout);
            }
            return OperateResult.CreateSuccessResult(bufferArray.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 接收一条完整的 <seealso cref="INetMessage" /> 数据内容，需要指定超时时间，单位为毫秒。
    /// </summary>
    /// <param name="timeout">超时时间，单位：毫秒</param>
    /// <param name="netMessage">消息的格式定义</param>
    /// <returns>带有是否成功的byte数组对象</returns>
    private async Task<OperateResult<byte[]>> ReceiveByMessageAsync(int timeout, INetMessage? netMessage)
    {
        if (netMessage == null)
        {
            return await ReceiveAsync(-1, timeout).ConfigureAwait(false);
        }

        if (netMessage.ProtocolHeadBytesLength < 0)
        {
            var headCode = BitConverter.GetBytes(netMessage.ProtocolHeadBytesLength);
            var codeLength = headCode[3] & 0xF;
            var receive = codeLength switch
            {
                1 => await ReceiveCommandLineFromPipeAsync(headCode[1], timeout).ConfigureAwait(false),
                2 => await ReceiveCommandLineFromPipeAsync(headCode[1], headCode[0], timeout).ConfigureAwait(false),
                _ => null,
            };
            if (receive == null)
            {
                return new OperateResult<byte[]>((int)CommErrorCode.CommandLengthCheckFailed, "Receive by specified code failed, length check failed");
            }
            if (!receive.IsSuccess)
            {
                return receive;
            }

            netMessage.HeadBytes = receive.Content;
            if (netMessage is SpecifiedCharacterMessage message)
            {
                if (message.EndLength == 0)
                {
                    return receive;
                }
                var endResult = await ReceiveAsync(message.EndLength, timeout).ConfigureAwait(false);
                if (!endResult.IsSuccess)
                {
                    return endResult;
                }
                return OperateResult.CreateSuccessResult(CollectionUtils.SpliceArray(receive.Content, endResult.Content));
            }
            return receive;
        }

        var headResult = await ReceiveAsync(netMessage.ProtocolHeadBytesLength, timeout).ConfigureAwait(false);
        if (!headResult.IsSuccess)
        {
            return headResult;
        }

        var start = netMessage.PependedUselesByteLength(headResult.Content);
        var cycleCount = 0;
        while (start >= netMessage.ProtocolHeadBytesLength)
        {
            headResult = await ReceiveAsync(netMessage.ProtocolHeadBytesLength, timeout).ConfigureAwait(false);
            if (!headResult.IsSuccess)
            {
                return headResult;
            }
            start = netMessage.PependedUselesByteLength(headResult.Content);
            cycleCount++;
            if (cycleCount > 10)
            {
                break;
            }
        }
        if (start > 0)
        {
            var head2Result = await ReceiveAsync(start, timeout).ConfigureAwait(false);
            if (!head2Result.IsSuccess)
            {
                return head2Result;
            }
            headResult.Content = CollectionUtils.SpliceArray(headResult.Content.RemoveBegin(start), head2Result.Content);
        }

        netMessage.HeadBytes = headResult.Content;
        var contentLength = netMessage.GetContentLengthByHeadBytes();
        if (contentLength <= 0)
        {
            return OperateResult.CreateSuccessResult(headResult.Content);
        }

        var result = new byte[netMessage.HeadBytes.Length + contentLength];
        netMessage.HeadBytes.CopyTo(result, 0);
        OperateResult contentResult = await ReceiveAsync(result, netMessage.HeadBytes.Length, contentLength, timeout).ConfigureAwait(false);
        if (!contentResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(contentResult);
        }
        return OperateResult.CreateSuccessResult(result);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
