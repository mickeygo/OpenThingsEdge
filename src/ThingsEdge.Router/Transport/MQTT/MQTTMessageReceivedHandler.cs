using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

internal sealed class MQTTMessageReceivedHandler : IMQTTMessageReceivedHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public MQTTMessageReceivedHandler(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(MqttApplicationMessageReceivedEventArgs message, CancellationToken cancellationToken)
    {
        try
        {
            // 发送请求给唯一处理者。
            var resp = await _mediator.Send(new RequestMQTTMessage(message), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 发送消息异常。");
        }

        try
        {
            // 广播消息给所有订阅者。
            await _mediator.Publish(new RequestMQTTMessage(message), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 推送消息异常。");
        }
    }
}
