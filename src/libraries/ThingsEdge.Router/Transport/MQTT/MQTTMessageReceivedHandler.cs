using MQTTnet.Client;
using ThingsEdge.Contracts.MQTT;

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
        var clientId = args.ClientId;
        var topic = args.ApplicationMessage.Topic;
        var body = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

        MQTTRequestMessageResult reqResult;
        try
        {
            // 发送请求给唯一处理者。
            // 第三方需要有接口处理该消息。
            reqResult = await _mediator.Send(new MQTTRequestMessage(clientId, topic, body), cancellationToken);
            if (!reqResult.IsSuccess())
            {
                return;
            }

            // TODO: 将数据统一处理。
            
            if (reqResult.AutoAcknowledge)
            {
                await args.AcknowledgeAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 发送消息异常。");
            return;
        }

        if (reqResult.RequestMessage != null)
        {
            try
            {
                // 广播消息给所有订阅者。
                await _mediator.Publish(new NotificationMessage(reqResult.RequestMessage), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTTMessageReceivedHandler] MQTT 推送消息异常。");
            }
        }
    }
}
