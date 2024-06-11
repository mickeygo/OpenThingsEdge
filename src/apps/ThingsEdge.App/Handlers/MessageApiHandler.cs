using ThingsEdge.Contracts;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.App.Handlers;

/// <summary>
/// 设备消息请求预处理，用于异步处理通知消息，如警报、设备状态信息。
/// </summary>
internal sealed class MessageApiHandler : IDirectMessageRequestApi
{
    public Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
