namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 表示 Native 转发服务基类。
/// </summary>
public abstract class AbstractNativeForwarder : IForwarder
{
    public ForworderSource Source => ForworderSource.Native;

    public abstract Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
