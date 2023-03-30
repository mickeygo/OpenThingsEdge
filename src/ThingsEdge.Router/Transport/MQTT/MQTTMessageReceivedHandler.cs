using MQTTnet.Client;

namespace ThingsEdge.Router.Transport.MQTT;

/// <summary>
/// 处理MQTT推送过来的消息。
/// </summary>
internal sealed class MQTTMessageReceivedHandler : IMQTTMessageReceivedHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public MQTTMessageReceivedHandler(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(MqttApplicationMessageReceivedEventArgs args, CancellationToken cancellationToken)
    {
        var topic = args.ApplicationMessage.Topic;
        var body = Encoding.UTF8.GetString(args.ApplicationMessage.CorrelationData);

        MQTTRequestMessageResult msgResult;
        try
        {
            // 发送请求给唯一处理者。
            // 第三方需要有接口处理该消息。
            msgResult = await _mediator.Send(new MQTTRequestMessage(topic, body), cancellationToken);
            if (!msgResult.IsSuccess)
            {
                return;
            }

            // TODO: 将数据发送给远程服务。

            if (msgResult.AutoAcknowledge)
            {
                await args.AcknowledgeAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 发送消息异常。");
            return;
        }

        if (msgResult.RequestMessage != null)
        {
            try
            {
                // 广播消息给所有订阅者。
                await _mediator.Publish(new NotificationMessage(msgResult.RequestMessage), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 推送消息异常。");
            }
        }
    }
}
