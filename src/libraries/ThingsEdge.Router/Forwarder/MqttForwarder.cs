namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 基于 MQTT 协议的转发数据。
/// </summary>
internal sealed class MqttForwarder : IForwarder
{
    public Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        var result = ResponseResult.FromOk(new ResponseMessage
        {
            Request = message,
            State = 0,
            CallbackItems = new(0),
        });
        return Task.FromResult(result);
    }
}
